using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Areas.Admin.Controllers;

public class UsersController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}