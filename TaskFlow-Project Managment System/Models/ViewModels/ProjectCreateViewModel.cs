using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class ProjectCreateViewModel
    {
        [Required(ErrorMessage = "Enter a project name.")]
        [StringLength(100, ErrorMessage = "The project name cannot exceed 100 characters.")]
        [Display(Name = "Project name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "The description cannot exceed 1,000 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [EnumDataType(typeof(ProjectTemplate))]
        [Display(Name = "Project template")]
        public ProjectTemplate Template { get; set; } = ProjectTemplate.Basic;

        [Range(1, int.MaxValue, ErrorMessage = "Choose a workspace.")]
        [Display(Name = "Workspace")]
        public int WorkspaceId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose a project creator.")]
        [Display(Name = "Created by")]
        public int CreatedByUserId { get; set; }

        public IReadOnlyList<SelectListItem> Workspaces { get; set; } = [];
        public IReadOnlyList<SelectListItem> Users { get; set; } = [];
    }
}
