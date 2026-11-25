using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Project_Managment_System.Models
{
    public class WorkspaceInvitation
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;

        public int WorkspaceId { get; set; }
        public WorkSpace WorkSpace { get; set; } = null!;

        public int InvitedByUserId { get; set; }
        public User InvitedByUser { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}