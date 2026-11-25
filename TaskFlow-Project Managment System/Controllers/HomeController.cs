using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_Project_Managment_System.Services;

namespace TaskFlow_Project_Managment_System.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly WorkFlowContext _db;
        private readonly IPermissionService _permissionService;

        public HomeController(WorkFlowContext db, IPermissionService permissionService)
        {
            _db = db;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Forbid();

            var workspaceId = await _db.Users
                .Where(user => user.Id == userId)
                .Select(user => user.WorkspaceId)
                .SingleOrDefaultAsync();

            if (!workspaceId.HasValue || !await _permissionService.HasPermissionAsync(
                    userId, workspaceId.Value, WorkspacePermission.ViewDashboard))
                return Forbid();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
