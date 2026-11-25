namespace TaskFlow_Project_Managment_System.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public WorkSpace WorkSpace { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}