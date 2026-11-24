using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public ProjectTemplate Template { get; init; }
        public DateTime CreatedAt { get; init; }
        public string WorkspaceName { get; init; } = string.Empty;
        public string CreatorName { get; init; } = string.Empty;
    }
}
