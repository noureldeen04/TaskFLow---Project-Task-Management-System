namespace TaskFlow_Project_Managment_System.Models
{
    public class WorkSpace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<User> Members { get; set; } = new List<User>();      // "contains" users
        public ICollection<Project> Projects { get; set; } = new List<Project>(); // "contains" projects
        public ICollection<Team> Teams { get; set; } = new List<Team>();       // "contains" teams
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();   // "contains" ActivityLog
        public ICollection<WorkspaceInvitation> Invitations { get; set; } = new List<WorkspaceInvitation>();   // "contains" WorkspaceInvitation
    }
 }
