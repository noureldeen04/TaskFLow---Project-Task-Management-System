using TaskFlow_Project_Managment_System.Models;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class WorkspaceSwitcherViewModel
    {
        public List<WorkSpace> Workspaces { get; set; } = new();
        public int? SelectedWorkspaceId { get; set; }
    }
}