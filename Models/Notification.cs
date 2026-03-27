using System;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem_DotNetMVC.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int? UserId { get; set; } // Specific user (if null, broadcast)

        public string? TargetRole { get; set; } // "All", "User", "Agent"

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}