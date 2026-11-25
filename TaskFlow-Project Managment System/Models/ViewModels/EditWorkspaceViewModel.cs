using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class EditWorkspaceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Workspace name is required.")]
        [StringLength(100, ErrorMessage = "Workspace name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}