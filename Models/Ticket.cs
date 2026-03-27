using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketingSystem_DotNetMVC.Models
{
    public enum TicketPriority
    {
        Low,
        Medium,
        High
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Closed
    }

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
        public TicketPriority Priority { get; set; } = TicketPriority.Low;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public User User { get; set; } = null!;

        public int? AssignedAgentId { get; set; }
        [ForeignKey("AssignedAgentId")]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public User? AssignedAgent { get; set; }

        public double Rating { get; set; } = 0;

        public string? Feedback { get; set; }

        public string? Attachments { get; set; } // JSON array of file paths

        // Navigation Properties
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
