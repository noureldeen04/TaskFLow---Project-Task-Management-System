using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.ViewModels;

namespace TaskFlow_Project_Managment_System.Controllers
{
    public class WorkSpaceController : Controller
    {
        private readonly WorkFlowContext _context;

        public WorkSpaceController(WorkFlowContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var workspaces = await _context.WorkSpaces
                .Where(workspace => workspace.OwnerId == 1)
                .Include(workspace => workspace.Teams)
                .Include(workspace => workspace.Projects)
                .OrderByDescending(workspace => workspace.CreatedAt)
                .ToListAsync();

            return View(workspaces);
        }

        [HttpGet]
        public async Task<IActionResult> Select(int id)
        {
            var workspaceExists = await _context.WorkSpaces
                .AnyAsync(workspace =>
                    workspace.Id == id && workspace.OwnerId == 1);

            if (!workspaceExists)
            {
                return NotFound();
            }

            Response.Cookies.Append(
                "SelectedWorkspaceId",
                id.ToString(),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var workspace = await _context.WorkSpaces
                .Include(workspace => workspace.Members)
                .Include(workspace => workspace.Projects)
               .Include(workspace => workspace.Teams)
                    .ThenInclude(team => team.Members)
                .Include(workspace => workspace.ActivityLogs)
                    .ThenInclude(activity => activity.User)
                .Include(workspace => workspace.Invitations)
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == id && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            return View(workspace);
        }

        public IActionResult Create()
        {
            return View(new CreateWorkspaceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkspaceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["WorkspaceError"] =
                    "Please enter a valid Workspace name and description.";

                return RedirectToAction(nameof(Index));
            }

            var workspace = new WorkSpace
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                OwnerId = 1
            };

            _context.WorkSpaces.Add(workspace);
            await _context.SaveChangesAsync();

            var activity = new ActivityLog
            {
                WorkspaceId = workspace.Id,
                UserId = 1,
                Action = "Workspace Created",
                Description = $"Workspace '{workspace.Name}' was created."
            };

            _context.ActivityLogs.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == id && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            var model = new EditWorkspaceViewModel
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Description = workspace.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditWorkspaceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == model.Id && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            workspace.Name = model.Name.Trim();
            workspace.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspace.Id,
                UserId = 1,
                Action = "Workspace Updated",
                Description = $"Workspace '{workspace.Name}' was updated."
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = workspace.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workspace = await _context.WorkSpaces
                .Include(workspace => workspace.Members)
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == id && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            foreach (var member in workspace.Members)
            {
                member.WorkspaceId = null;
            }

            _context.WorkSpaces.Remove(workspace);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> InviteMember(int workspaceId)
        {
            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == workspaceId && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            var model = new InviteMemberViewModel
            {
                WorkspaceId = workspace.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteMember(InviteMemberViewModel model)
        {
            var workspace = await _context.WorkSpaces
                .FirstOrDefaultAsync(workspace =>
                    workspace.Id == model.WorkspaceId && workspace.OwnerId == 1);

            if (workspace == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["InvitationError"] =
                    "Please enter a valid email address.";

                return RedirectToAction(nameof(Details), new { id = workspace.Id });
            }

            var email = model.Email.Trim().ToLowerInvariant();

            var invitationExists = await _context.WorkspaceInvitations
                .AnyAsync(invitation =>
                    invitation.WorkspaceId == workspace.Id &&
                    invitation.Email == email &&
                    invitation.Status == "Pending");

            if (invitationExists)
            {
                TempData["InvitationError"] =
                    "A pending invitation already exists for this email.";

                return RedirectToAction(nameof(Details), new { id = workspace.Id });
            }

            _context.WorkspaceInvitations.Add(new WorkspaceInvitation
            {
                WorkspaceId = workspace.Id,
                InvitedByUserId = 1,
                Email = email
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspace.Id,
                UserId = 1,
                Action = "Member Invited",
                Description = $"An invitation was created for {email}."
            });

            await _context.SaveChangesAsync();

            TempData["InvitationSuccess"] = "Invitation saved as pending.";

            return RedirectToAction(nameof(Details), new { id = workspace.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            var invitation = await _context.WorkspaceInvitations
                .Include(invitation => invitation.WorkSpace)
                .FirstOrDefaultAsync(invitation =>
                    invitation.Id == id &&
                    invitation.WorkSpace.OwnerId == 1);

            if (invitation == null)
            {
                return NotFound();
            }

            var workspaceId = invitation.WorkspaceId;
            var email = invitation.Email;

            _context.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspaceId,
                UserId = 1,
                Action = "Invitation Removed",
                Description = $"The pending invitation for {email} was removed."
            });

            _context.WorkspaceInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            TempData["InvitationSuccess"] =
                "The pending invitation was removed.";

            return RedirectToAction(nameof(Details), new { id = workspaceId });
        }
    }
}