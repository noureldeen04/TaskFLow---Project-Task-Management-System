using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Models.Entities
{
    public class WorkspaceMember
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }
        public WorkSpace Workspace { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public WorkspaceRole Role { get; set; }
    }
}