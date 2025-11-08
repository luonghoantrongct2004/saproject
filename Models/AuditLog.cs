using System.ComponentModel.DataAnnotations;

namespace SAProject.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string? UserName { get; set; }

        [Required]
        public string Action { get; set; } // Ví dụ: "Create Product", "Login Success", "Delete Product"

        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }

        public string? RequestPath { get; set; }

        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
