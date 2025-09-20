using COMP2139_ICE1.Models;
using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Controllers;

public class ProjectController : Controller
{
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        var projects = new List<Project>()
        {
            new Project { ProjectId = 1, Name = "Project 1", Description = "First Project" },
            // Feel free to add more projects here
        };
        return View(projects);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Create(Project project)
    {
        // more logic in here
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public IActionResult Details(int id)
    {
        var project = new Project{  ProjectId = id, Name = "Project" + id, Description = "Details of Project" + id };
        return View(project);
    }
}