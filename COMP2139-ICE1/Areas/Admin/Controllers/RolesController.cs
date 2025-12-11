using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing roles within the Admin area.
    /// Ensures that only users with the "Admin" role can access these functionalities.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Constructor for RolesController, injecting RoleManager for role management operations.
        /// </summary>
        /// <param name="roleManager">The RoleManager instance used for managing roles.</param>
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Displays a list of all existing roles.
        /// GET: /Admin/Roles
        /// </summary>
        /// <returns>An IActionResult displaying the list of roles.</returns>
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        /// <summary>
        /// Displays the view for creating a new role.
        /// GET: /Admin/Roles/Create
        /// </summary>
        /// <returns>An IActionResult displaying the Create Role form.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles the POST request for creating a new role.
        /// Validates the role name and creates the role if successful.
        /// POST: /Admin/Roles/Create
        /// </summary>
        /// <param name="name">The name of the new role to create.</param>
        /// <returns>Redirects to the Index page if successful, otherwise returns to the Create view with errors.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Error = "Role name is required.";
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(name));

            if (result.Succeeded)
                return RedirectToAction("Index");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        /// <summary>
        /// Displays the confirmation view for deleting a role.
        /// GET: /Admin/Roles/Delete/{id}
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>An IActionResult displaying the Delete Role confirmation form, or NotFound if the role does not exist.</returns>
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        /// <summary>
        /// Handles the POST request for deleting a role.
        /// Prevents deletion of the "Admin" role and deletes other roles if successful.
        /// POST: /Admin/Roles/Delete/{id}
        /// </summary>
        /// <param name="id">The ID of the role to be deleted.</param>
        /// <returns>Redirects to the Index page if successful, otherwise returns to the Index view with errors.</returns>
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Role id is required.");

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound();

            // Prevent deletion of the "Admin" role
            if (role.Name == "Admin")
            {
                ModelState.AddModelError("", "The Admin role cannot be deleted.");
                return View("Index", _roleManager.Roles.ToList());
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            return View("Index", _roleManager.Roles.ToList());
        }
    }
}