using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketingSystem_DotNetMVC.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Low"; // Low, Medium, High

        [Required]
        public string Status { get; set; } = "Open"; // Open, In Progress, Closed

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? AssignedAgentId { get; set; }
        [ForeignKey("AssignedAgentId")]
        public User? AssignedAgent { get; set; }

        public double Rating { get; set; } = 0;

        public string? Feedback { get; set; }

        public string? Attachments { get; set; } // JSON array of file paths

        // Navigation Properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
