using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class CreateTeamViewModel
    {
        public int WorkspaceId { get; set; }

        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
