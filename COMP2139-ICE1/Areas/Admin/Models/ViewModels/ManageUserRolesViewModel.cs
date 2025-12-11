namespace COMP2139_ICE1.Areas.Admin.Models.ViewModels;

/// <summary>
/// ViewModel for managing user roles in the Admin area.
/// Used to display a user's current roles and available roles for assignment.
/// </summary>
public class ManageUserRolesViewModel
{
    /// <summary>
    /// Gets or sets the ID of the user whose roles are being managed.
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the email of the user whose roles are being managed.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the list of all available roles in the system.
    /// </summary>
    public List<string> AvailableRoles { get; set; }
    
    /// <summary>
    /// Gets or sets the list of roles currently assigned to the user.
    /// </summary>
    public IList<string> AssignedRoles { get; set; }

    /// <summary>
    /// Gets or sets the list of roles selected by the administrator to be assigned to the user.
    /// This property is used during the POST request to capture changes.
    /// </summary>
    public List<string> SelectedRoles { get; set; }
}