using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.Entities;
using TaskFlow_Project_Managment_System.ModelView;
using PriorityEnum = TaskFlow_Project_Managment_System.Models.Enums.TaskPriority.Taskpriority;
using StatusEnum = TaskFlow_Project_Managment_System.Models.Enums.TaskStatus.Taskstatus;
using UserEntity = TaskFlow_Project_Managment_System.Models.User;

namespace TaskFlow_Project_Managment_System.Controllers
{
    // Boards & Tasks module (Menna).
    // GET /Tasks            -> the Kanban board page
    // /api/tasks/...        -> JSON endpoints used by wwwroot/js/taskflow-board.js
    public class TasksController : Controller
    {
        private readonly WorkFlowContext _db;

        public TasksController(WorkFlowContext db)
        {
            _db = db;
        }

        // ---------- Page ----------
        public IActionResult Index() => View();

        // ---------- API ----------
        [HttpGet("api/tasks")]
        public async Task<IActionResult> GetAll([FromQuery] bool archived = false)
        {
            var tasks = await _db.Tasks
                .Include(t => t.AssignedByUser)
                .Where(t => t.Archived == archived)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return Ok(tasks.Select(ToDto));
        }

        [HttpPost("api/tasks")]
        public async Task<IActionResult> Create([FromBody] TaskDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            // TEMPORARY: until the Users and Projects modules exist, tasks are attached
            // to a default creator and a default project (see helpers below).
            var creator = await GetOrCreateUserAsync("Menna");
            var projectId = await GetDefaultProjectIdAsync(creator.Id);

            var task = new AllTask
            {
                ProjectId = projectId,
                CreatedByUserId = creator.Id
            };
            await ApplyAsync(task, dto);

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return Ok(new { id = task.Id });
        }

        [HttpPut("api/tasks/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            await ApplyAsync(task, dto);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Drag & drop between columns
        [HttpPatch("api/tasks/{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] string status)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Status = ParseStatus(status);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // value=true -> archive, value=false -> restore
        [HttpPatch("api/tasks/{id:int}/archive")]
        public async Task<IActionResult> SetArchive(int id, [FromQuery] bool value = true)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Archived = value;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("api/tasks/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Mapping helpers ----------
        private static TaskDto ToDto(AllTask t) => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = StatusToString(t.Status),
            Priority = t.Priority.ToString(),
            Assignee = t.AssignedByUser?.FullName,
            DueDate = t.DueDate,
            Archived = t.Archived
        };

        private async Task ApplyAsync(AllTask task, TaskDto dto)
        {
            task.Title = dto.Title.Trim();
            task.Description = dto.Description;
            task.Status = ParseStatus(dto.Status);
            task.Priority = ParsePriority(dto.Priority);
            task.DueDate = dto.DueDate;

            if (string.IsNullOrWhiteSpace(dto.Assignee))
                task.AssignedToUserId = null;
            else
                task.AssignedToUserId = (await GetOrCreateUserAsync(dto.Assignee.Trim())).Id;
        }

        private static StatusEnum ParseStatus(string? value) =>
            (value ?? string.Empty).ToLowerInvariant() switch
            {
                "progress" => StatusEnum.InProgress,
                "review" => StatusEnum.Review,
                "done" => StatusEnum.Done,
                _ => StatusEnum.ToDo
            };

        private static string StatusToString(StatusEnum status) =>
            status switch
            {
                StatusEnum.InProgress => "progress",
                StatusEnum.Review => "review",
                StatusEnum.Done => "done",
                _ => "todo"
            };

        private static PriorityEnum ParsePriority(string? value) =>
            Enum.TryParse<PriorityEnum>(value, true, out var result) && Enum.IsDefined(result)
                ? result
                : PriorityEnum.Medium;

        // ---------- TEMPORARY helpers (remove when Users / Projects modules are ready) ----------
        private async Task<UserEntity> GetOrCreateUserAsync(string fullName)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.FullName == fullName);
            if (user != null) return user;

            user = new UserEntity
            {
                FullName = fullName,
                Email = fullName.ToLowerInvariant().Replace(" ", ".") + "@taskflow.local"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        private async Task<int> GetDefaultProjectIdAsync(int ownerId)
        {
            var existingId = await _db.Projects
                .OrderBy(p => p.Id)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
            if (existingId != null) return existingId.Value;

            var workspace = new WorkSpace { Name = "Default workspace", OwnerId = ownerId };
            _db.WorkSpaces.Add(workspace);
            await _db.SaveChangesAsync();

            var project = new Project
            {
                Name = "Default project",
                WorkspaceId = workspace.Id,
                CreatedByUserId = ownerId
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project.Id;
        }
    }
}
