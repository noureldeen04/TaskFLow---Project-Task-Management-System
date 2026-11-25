using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Models.Entities
{
    public class ProjectPermissionSettings
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public ProjectAccessLevel ViewAccess { get; set; } = ProjectAccessLevel.WorkspaceMembers;
        public ProjectAccessLevel EditAccess { get; set; } = ProjectAccessLevel.ProjectCreatorOnly;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
