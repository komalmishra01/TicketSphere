using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketingSystem_DotNetMVC.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
