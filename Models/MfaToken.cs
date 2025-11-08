using System.ComponentModel.DataAnnotations;

namespace SAProject.Models;

public class MfaToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
