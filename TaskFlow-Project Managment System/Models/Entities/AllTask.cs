using PriorityEnum = TaskFlow_Project_Managment_System.Models.Enums.TaskPriority.Taskpriority;
using StatusEnum = TaskFlow_Project_Managment_System.Models.Enums.TaskStatus.Taskstatus;

namespace TaskFlow_Project_Managment_System.Models.Entities
{
    public class AllTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int ProjectId { get; set; }//FK From Project
        public  Project projects { get; set; } = null!;// project nav


        public int CreatedByUserId { get; set; }// Fk From User
        public User CreatedByUser { get; set; } = null!;//user nav

        public int? AssignedToUserId { get; set; }//FK From User

        public User AssignedByUser { get; set; } = null!;//user nav

       

        // ---- Added by Menna for the Boards & Tasks module ----
        public StatusEnum Status { get; set; } = StatusEnum.ToDo;
        public PriorityEnum Priority { get; set; } = PriorityEnum.Medium;
        public bool Archived { get; set; }

        public DateTime? DueDate { get; set; } // Optional: not every task has a deadline.


    }
}
