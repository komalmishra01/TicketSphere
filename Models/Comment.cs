using System.ComponentModel.DataAnnotations;

namespace TicketingSystem_DotNetMVC.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public string CreatedBy { get; set; } = string.Empty; // Username or UserID

        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; } = null!;
    }
}
