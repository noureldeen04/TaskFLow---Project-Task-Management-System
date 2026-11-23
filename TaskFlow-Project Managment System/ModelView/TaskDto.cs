namespace TaskFlow_Project_Managment_System.ModelView
{
    // The shape of a task as the Boards & Tasks page (JavaScript) sends and receives it.
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "todo";        // todo | progress | review | done
        public string Priority { get; set; } = "Medium";    // Low | Medium | High | Critical
        public string? Assignee { get; set; }               // user full name
        public DateTime? DueDate { get; set; }
        public bool Archived { get; set; }
    }
}
