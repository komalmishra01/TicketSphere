using System.ComponentModel.DataAnnotations;

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
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public string? AttachmentPath { get; set; }

        // Foreign Key to User (Assuming simple string for now, can be linked to IdentityUser later)
        public string UserId { get; set; } = string.Empty;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
