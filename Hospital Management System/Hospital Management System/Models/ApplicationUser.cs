using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string? FullName { get; set; }

    [PersonalData]
    public string? Role { get; set; }

    [PersonalData]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}