using COMP2139_ICE1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace COMP2139_ICE1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("picture/{userId}")]
    public async Task<IActionResult> GetProfilePicture(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || user.ProfilePicture == null || user.ProfilePicture.Length == 0)
        {
            return NotFound();
        }

        return File(user.ProfilePicture, user.ProfilePictureMimeType ?? "image/jpeg");
    }
}