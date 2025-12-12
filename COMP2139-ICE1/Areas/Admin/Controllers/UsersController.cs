using COMP2139_ICE1.Areas.Admin.Models.ViewModels;
using COMP2139_ICE1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing users and their roles within the Admin area.
    /// Ensures that only users with the "Admin" role can access these functionalities.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Constructor for UsersController, injecting UserManager and RoleManager for user and role management operations.
        /// </summary>
        /// <param name="userManager">The UserManager instance used for managing users.</param>
        /// <param name="roleManager">The RoleManager instance used for managing roles.</param>
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Displays a list of all registered users along with their assigned roles.
        /// </summary>
        /// <returns>An IActionResult displaying the list of users and their roles.</returns>
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var vm = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                vm.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return View(vm);
        }

        /// <summary>
        /// Displays the view for managing roles of a specific user.
        /// GET: /Admin/Users/Manage/{id}
        /// </summary>
        /// <param name="id">The ID of the user whose roles are to be managed.</param>
        /// <returns>An IActionResult displaying the Manage User Roles form, or NotFound/BadRequest if the user is not found or ID is missing.</returns>
        public async Task<IActionResult> Manage(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User id is required.");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var vm = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList(),
                AssignedRoles = await _userManager.GetRolesAsync(user),
            };

            return View(vm);
        }

        /// <summary>
        /// Handles the POST request for updating a user's roles.
        /// Removes existing roles and assigns selected new roles to the user.
        /// POST: /Admin/Users/Manage
        /// </summary>
        /// <param name="vm">The ViewModel containing the user ID and selected roles.</param>
        /// <returns>Redirects to the Index page if successful, otherwise returns to the Manage view with errors or BadRequest/NotFound.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageUserRolesViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var user = await _userManager.FindByIdAsync(vm.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove user from all current roles
            if (currentRoles != null && currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add user to selected roles
            if (vm.SelectedRoles != null && vm.SelectedRoles.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, vm.SelectedRoles);
            }

            return RedirectToAction("Index");
        }
    }
}