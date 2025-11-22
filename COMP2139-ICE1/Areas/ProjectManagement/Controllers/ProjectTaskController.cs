using COMP2139_ICE1.Areas.ProjectManagement.Models;
using COMP2139_ICE1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE1.Areas.ProjectManagement.Controllers;

[Area("ProjectManagement")]
[Route("[area]/[controller]/[action]")]
public class ProjectTaskController : Controller
{
    private readonly ApplicationDbContext _context; 
    public ProjectTaskController(ApplicationDbContext context) : base() 
    { 
        _context = context; 
    } 

    // Displays a list of tasks that belong to a specific project, based on the projectId passed in the URL.
    [HttpGet("Index/{projectId:int}")] 
    public IActionResult Index(int projectId) 
    { 
        var tasks = _context.ProjectTasks 
                            .Where(t => t.ProjectId == projectId) 
                            .ToList(); 
        
        ViewBag.ProjectId = projectId;   
        return View(tasks); 
    } 

    // Shows detailed information about a single task, based on the task id.
    [HttpGet("Details/{Id:int}")] 
    public IActionResult Details(int id) 
    { 
        var task = _context.ProjectTasks
                .Include(t => t.Project) 
                // Include related project data 
                .FirstOrDefault(t => t.ProjectTaskId == id); 

        if (task == null) 
        { 
            return NotFound(); 
        } 
        
        return View(task); 
    } 

    // Shows the form for creating a new task under a specific project
    [HttpGet("Create/{projectId:int}")] 
    public IActionResult Create(int projectId) 
    { 
        var project = _context.Projects.Find(projectId); 
        if (project == null) 
        { 
            return NotFound(); 
        } 
        
        var task = new ProjectTask 
        { 
            ProjectId = projectId, 
            Title = "", 
            Description = "" 
        }; 

        return View(task); 
    } 

    // Saves the submitted task form to the database.
    [HttpPost("Create/{projectId:int}")] 
    [ValidateAntiForgeryToken] 
    public IActionResult Create([Bind("Title", "Description", "ProjectId")] ProjectTask task) 
    { 
        if (ModelState.IsValid) 
        { 
            _context.ProjectTasks.Add(task); 
            _context.SaveChanges(); 
            
            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId }); 
        } 

        ViewBag.Projects = new SelectList(_context.Projects, "ProjectId", "Name", task.ProjectId); 
        return View(task); 
    } 

 
    // Loads the existing task data and displays it in an edit form so the user can modify it.
    [HttpGet("Edit/{id:int}")] 
    public IActionResult Edit(int id) 
    { 
        var task = _context.ProjectTasks 
                            .Include(t => t.Project) 
                            // Include related project data 
                            .FirstOrDefault(t => t.ProjectTaskId == id); 
        
        if (task == null) 
        { 
            return NotFound(); 
        } 
        
        ViewBag.Projects = new SelectList(_context.Projects, "ProjectId", "Name", task.ProjectId); 
        return View(task); 
    } 

    // Receives the updated task data from the form, saves the changes to the database, and redirects back to the task list.
    [HttpPost("Edit/{id:int}")] 
    [ValidateAntiForgeryToken] 
    public IActionResult Edit(int id, [Bind("ProjectTaskId", "Title", "Description", "ProjectId")] ProjectTask task) 
    { 
        if (id != task.ProjectTaskId) 
        { 
            return NotFound(); 
        } 

        if (ModelState.IsValid) 
        { 
            _context.ProjectTasks.Update(task); 
            _context.SaveChanges(); 
            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId }); 
        } 
        
        ViewBag.Projects = new SelectList(_context.Projects, "ProjectId", "Name", task.ProjectId); 
        return View(task); 
    } 
    
    // Displays a confirmation page showing the task details before deleting it.
    [HttpGet("Delete/{id:int}")] 
    public IActionResult Delete(int id) 
    { 
        var task = _context.ProjectTasks 
                            .Include(t => t.Project) 
                            // Include related project data 
                            .FirstOrDefault(t => t.ProjectTaskId == id); 

        if (task == null) 
        { 
            return NotFound(); 
        } 

        return View(task); 
    } 
    
    // Deletes the selected task from the database and redirects back to the project’s task list.
    [HttpPost("DeleteConfirmed/{projectTaskId:int}")] 
    [ValidateAntiForgeryToken] 
    public IActionResult DeleteConfirmed(int projectTaskId) 
    {
        var task = _context.ProjectTasks.Find(projectTaskId); 
        if (task != null) 
        { 
            _context.ProjectTasks.Remove(task); 
            _context.SaveChanges(); 
            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId }); 
        } 
        return NotFound(); 
    }
    
    // Searches tasks based on a search string and returns a filtered task list to the same Index view.
    // GET: ProjectTasks/Search/{projectId?}/{searchString?}
    [HttpGet("Search")]
    public async Task<IActionResult> Search(int? projectId, string searchString)
    {
        // Start with all tasks as an IQueryable query (deferred execution)
        var taskQuery = _context.ProjectTasks.AsQueryable();

        // Track whether a search was performed
        bool searchPerformed = !string.IsNullOrWhiteSpace(searchString);

        // If a projectId is provided, filter by project
        if (projectId.HasValue)
        {
            taskQuery = taskQuery.Where(t => t.ProjectId == projectId.Value);
        }

        // ❗ FIXED: Apply search filter when searchString is provided
        if (searchPerformed)
        {
            searchString = searchString.ToLower(); // Case-insensitive search

            // Ensure null-safe search on nullable Description
            taskQuery = taskQuery.Where(t =>
                t.Title.ToLower().Contains(searchString) ||
                (t.Description != null && t.Description.ToLower().Contains(searchString))
            );
        }

        // ❗ WHY ASYNC? ❗
        // The database query is executed asynchronously using `ToListAsync()`
        // This prevents blocking the main thread while waiting for the result.
        var tasks = await taskQuery.ToListAsync();

        // Pass search metadata to the view for UI updates
        ViewBag.ProjectId = projectId;
        ViewData["SearchPerformed"] = searchPerformed;
        ViewData["SearchString"] = searchString;

        // Reuse Index view to display filtered results
        return View("Index", tasks);
    }
}