using TaskFlow_Project_Managment_System.Models.Entities;

namespace TaskFlow_Project_Managment_System.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        public int WorkspaceId { get; set; } // FK From Workspace 
        public WorkSpace workSpaces { get; set; } = null!;//workspace navigation



        public int CreatedByUserId { get; set; } // FK From User 
        public User Users { get; set; } = null!;// user navigation

        public ICollection<AllTask> ProjectTasks { get; set; } = new List<AllTask>();

    }
}
