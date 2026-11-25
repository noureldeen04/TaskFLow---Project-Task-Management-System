using TaskFlow_Project_Managment_System.Models.Entities;

namespace TaskFlow_Project_Managment_System.Models
{
    public class User
    {

        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int? WorkspaceId { get; set; }
        public WorkSpace workSpaces { get; set; } = null!;



        public ICollection<WorkSpace> OwnedWorkspaces { get; set; } = new List<WorkSpace>();   // "owns"
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();       // "creates"
        public ICollection<AllTask> CreatedTasks { get; set; } = new List<AllTask>();        // "creates"
        public ICollection<AllTask> AssignedTasks { get; set; } = new List<AllTask>();       // "assigned_to"
    }
}
