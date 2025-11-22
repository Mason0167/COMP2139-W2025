using COMP2139_ICE1.Areas.ProjectManagement.Models;
using COMP2139_ICE1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE1.Areas.ProjectManagement.Controllers;

[Area("ProjectManagement")]
[Route("[area]/[controller]/[action]")]
public class ProjectController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProjectController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Retrieves all Project records from the database and converts the result into a list, then display.
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects.ToListAsync();
        
        return View(projects);
    }
    
    // Displays the empty form for creating a new project.
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // Handles the form submission when the user clicks "Save" on the create page.
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Project project)
    {
        if (ModelState.IsValid)
        {
            // Convert to UTC before saving
            project.StartDate = ToUtc(project.StartDate);
            project.EndDate = ToUtc(project.EndDate);
            
            _context.Projects.Add(project);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(project);
    }

    private DateTime ToUtc(DateTime input)
    {
        if (input.Kind == DateTimeKind.Utc)
        {
            return input;
        }

        if (input.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(input, DateTimeKind.Utc);
        }
        return input.ToUniversalTime();
    }
    
    
    [HttpGet("Details/{id:Int}")]
    public IActionResult Details(int id)
    {
        //var project = new Project{  ProjectId = id, Name = "Project" + id, Description = "Details of Project" + id };
        var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    [HttpGet("Edit/{id:Int}")]
    public IActionResult Edit(int id)
    {
        var project = _context.Projects.Find(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }
    
    [HttpPost("Edit/{id:Int}")] 
    [ValidateAntiForgeryToken] 
    public IActionResult Edit(int id, [Bind("ProjectId, Name, Description")] Project project) 
    { 
        if (id != project.ProjectId) 
        { 
            return NotFound(); 
        } 

        if (ModelState.IsValid) 
        { 
            try 
            { 
                _context.Projects.Update(project); 
                _context.SaveChanges(); 
            } 
            catch (DbUpdateConcurrencyException)
            // Detect error (Specific one)
            { 
                if (!ProjectExists(project.ProjectId)) 
                { 
                    return NotFound(); 
                } else 
                { 
                    throw; 
                } 
            } 
            return RedirectToAction("Index"); 
        } 
        return View(project); 
    } 

    private bool ProjectExists(int id) 
    { 
        return _context.Projects.Any(e => e.ProjectId == id); 
    } 



    [HttpGet("Delete/{id:Int}")]
    public IActionResult Delete(int id)
    {
        var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
        if(project == null)
        {
            return NotFound();
        }
        return View(project);
    }
    
    [HttpPost("DeleteConfirmed/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return NotFound();
    }
    
    // Custom route for search functionality
    // Accessible at /Projects/Search/{searchString?}
    [HttpGet("Search/{searchString?}")]
    public async Task<IActionResult> Search(string searchString)
    {
        // Fetch all projects from the database as an IQueryable collection
        // IQueryable allows us to apply filters before executing the database query
        var projectsQuery = _context.Projects.AsQueryable();

        // Check if a search string was provided (avoids null or empty search issues)
        bool searchPerformed = !string.IsNullOrWhiteSpace(searchString);

        if (searchPerformed)
        {
            // Convert searchString to lowercase to make the search case-insensitive
            searchString = searchString.ToLower();

            // Apply filtering: Match project name or description
            // Description is checked for null before calling ToLower() to prevent NullReferenceException
            projectsQuery = projectsQuery.Where(p =>
                p.Name.ToLower().Contains(searchString) ||
                (p.Description != null && p.Description.ToLower().Contains(searchString)));
        }

        // ❗ WHY ASYNC? ❗
        // Asynchronous execution means this method does not block the thread while waiting for the database.
        // Instead of blocking, ASP.NET Core can process other incoming requests while waiting for the result.
        // This improves scalability and application responsiveness.
    
        // Execute the query asynchronously using `ToListAsync()`
        var projects = await projectsQuery.ToListAsync();
    
        // ❗ HOW ASYNC WORKS HERE? ❗
        // `await` releases the current thread while waiting for the query execution to complete.
        // When the database call finishes, execution resumes on this method at this point.

        // Store search metadata for the view
        ViewData["SearchPerformed"] = searchPerformed;
        ViewData["SearchString"] = searchString;

        // Return the filtered list to the Index view (reusing existing UI)
        return View("Index", projects);
    }
}
