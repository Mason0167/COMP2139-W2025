using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Areas.Admin.Controllers;

public class RolesController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}