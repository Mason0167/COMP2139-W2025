namespace COMP2139_ICE1.Areas.Admin.Models.ViewModels;

/// <summary>
/// ViewModel for displaying user information along with their assigned roles.
/// Used in the Admin area for listing users and their roles.
/// </summary>
public class UserRoleViewModel
{
    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the list of roles assigned to the user.
    /// </summary>
    public IList<string> Roles { get; set; }
}