using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.ViewModels;

namespace TaskFlow_Project_Managment_System.Controllers
{
    public class TeamsController : Controller
    {
        private readonly WorkFlowContext _context;

        public TeamsController(WorkFlowContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int workspaceId)
        {
            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == workspaceId && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            var model = new CreateTeamViewModel
            {
                WorkspaceId = workspaceId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == model.WorkspaceId && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            var team = new Team
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                WorkspaceId = workspace.Id
            };

            _context.Teams.Add(team);

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspace.Id,
                UserId = 1,
                Action = "Team Created",
                Description = $"Team '{team.Name}' was created."
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "WorkSpace",
                new { id = workspace.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams
                .Include(team => team.WorkSpace)
                .FirstOrDefaultAsync(team =>
                    team.Id == id && team.WorkSpace.OwnerId == 1);

            if (team == null)
            {
                return NotFound();
            }

            var model = new EditTeamViewModel
            {
                Id = team.Id,
                WorkspaceId = team.WorkspaceId,
                Name = team.Name,
                Description = team.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTeamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var team = await _context.Teams
                .Include(team => team.WorkSpace)
                .FirstOrDefaultAsync(team =>
                    team.Id == model.Id && team.WorkSpace.OwnerId == 1);

            if (team == null)
            {
                return NotFound();
            }

            team.Name = model.Name.Trim();
            team.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = team.WorkspaceId,
                UserId = 1,
                Action = "Team Updated",
                Description = $"Team '{team.Name}' was updated."
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "WorkSpace",
                new { id = team.WorkspaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .Include(team => team.WorkSpace)
                .FirstOrDefaultAsync(team =>
                    team.Id == id && team.WorkSpace.OwnerId == 1);

            if (team == null)
            {
                return NotFound();
            }

            var workspaceId = team.WorkspaceId;

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspaceId,
                UserId = 1,
                Action = "Team Deleted",
                Description = $"Team '{team.Name}' was deleted."
            });

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "WorkSpace",
                new { id = workspaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(AddTeamMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["TeamMemberError"] = "Please select a valid team member.";
                return RedirectToAction("Details", "WorkSpace", new { id = model.WorkspaceId });
            }

            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == model.WorkspaceId &&
                    workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(team => team.Members)
                .FirstOrDefaultAsync(team =>
                    team.Id == model.TeamId &&
                    team.WorkspaceId == workspace.Id);

            if (team == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Id == model.UserId &&
                    user.WorkspaceId == workspace.Id);

            if (user == null)
            {
                TempData["TeamMemberError"] =
                    "The selected user is not a member of this Workspace.";

                return RedirectToAction("Details", "WorkSpace", new { id = workspace.Id });
            }

            if (team.Members.Any(member => member.Id == user.Id))
            {
                TempData["TeamMemberError"] =
                    "This user is already a member of the Team.";

                return RedirectToAction("Details", "WorkSpace", new { id = workspace.Id });
            }

            team.Members.Add(user);

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspace.Id,
                UserId = 1,
                Action = "Team Member Added",
                Description = $"{user.FullName} was added to Team '{team.Name}'."
            });

            await _context.SaveChangesAsync();

            TempData["TeamMemberSuccess"] =
                $"{user.FullName} was added to the Team.";

            return RedirectToAction("Details", "WorkSpace", new { id = workspace.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, int userId)
        {
            var team = await _context.Teams
                .Include(team => team.Members)
                .FirstOrDefaultAsync(team =>
                    team.Id == teamId &&
                    team.WorkSpace.OwnerId == 1);

            if (team == null)
            {
                return NotFound();
            }

            var member = team.Members
                .FirstOrDefault(member => member.Id == userId);

            if (member == null)
            {
                return NotFound();
            }

            team.Members.Remove(member);

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = team.WorkspaceId,
                UserId = 1,
                Action = "Team Member Removed",
                Description = $"{member.FullName} was removed from Team '{team.Name}'."
            });

            await _context.SaveChangesAsync();

            TempData["TeamMemberSuccess"] =
                $"{member.FullName} was removed from the Team.";

            return RedirectToAction(
                "Details",
                "WorkSpace",
                new { id = team.WorkspaceId });
        }
    }
}