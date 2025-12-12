using Microsoft.AspNetCore.Identity;

namespace COMP2139_ICE1.Models;

public class ApplicationUser : IdentityUser
{
    public byte[]? ProfilePicture { get; set; }
    public string? ProfilePictureMimeType { get; set; }
    public DateTime? LastPhoneNumberChangeDate { get; set; }
    public int PhoneNumberChangeCount { get; set; } = 0;
}