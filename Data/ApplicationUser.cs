using Microsoft.AspNetCore.Identity;

namespace SAProject.Data;
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public bool RequireMfa { get; set; } = false;
    public DateTime? MfaEnabledAt { get; set; }
    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LastFailedLoginDate { get; set; }
    public bool ForceMfaAfterFailedAttempts { get; set; } = true;
}
