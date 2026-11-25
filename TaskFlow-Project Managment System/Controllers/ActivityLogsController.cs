using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Services;

namespace TaskFlow_Project_Managment_System.Controllers
{
    [Authorize]
    public class ActivityLogsController : Controller
    {
        private readonly WorkFlowContext _db;
        private readonly IPermissionService _permissionService;

        public ActivityLogsController(WorkFlowContext db, IPermissionService permissionService)
        {
            _db = db;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Activity Logs";

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Forbid();

            var workspaceId = await _db.Users
                .Where(user => user.Id == userId)
                .Select(user => user.WorkspaceId)
                .SingleOrDefaultAsync();

            if (!workspaceId.HasValue || !await _permissionService.HasPermissionAsync(
                    userId, workspaceId.Value, WorkspacePermission.ViewActivityLogs))
                return Forbid();

            return View();
        }
    }
}
