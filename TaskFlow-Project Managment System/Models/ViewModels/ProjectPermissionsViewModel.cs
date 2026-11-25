using System.ComponentModel.DataAnnotations;
using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class ProjectPermissionsViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public bool IsEditing { get; set; }
        public bool IsPersisted { get; set; }

        [EnumDataType(typeof(ProjectAccessLevel))]
        [Display(Name = "View access")]
        public ProjectAccessLevel ViewAccess { get; set; } = ProjectAccessLevel.WorkspaceMembers;

        [EnumDataType(typeof(ProjectAccessLevel))]
        [Display(Name = "Edit access")]
        public ProjectAccessLevel EditAccess { get; set; } = ProjectAccessLevel.ProjectCreatorOnly;
    }
}
