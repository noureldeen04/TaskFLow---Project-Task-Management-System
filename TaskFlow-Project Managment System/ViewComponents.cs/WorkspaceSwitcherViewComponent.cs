using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models.ViewModels;

namespace TaskFlow_Project_Managment_System.ViewComponents
{
    public class WorkspaceSwitcherViewComponent : ViewComponent
    {
        private readonly WorkFlowContext _context;

        public WorkspaceSwitcherViewComponent(WorkFlowContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var workspaces = await _context.WorkSpaces
                .Where(workspace => workspace.OwnerId == 1)
                .OrderBy(workspace => workspace.Name)
                .ToListAsync();

            var routeValues = ViewComponentContext.ViewContext.RouteData.Values;
            var controller = routeValues["controller"]?.ToString();

            int? selectedWorkspaceId = null;

            if (controller == "WorkSpace" &&
                routeValues.TryGetValue("id", out var idValue) &&
                int.TryParse(idValue?.ToString(), out var workspaceId))
            {
                selectedWorkspaceId = workspaceId;
            }
            else if (controller == "Teams" &&
                     routeValues.TryGetValue("workspaceId", out var workspaceIdValue) &&
                     int.TryParse(workspaceIdValue?.ToString(), out var teamWorkspaceId))
            {
                selectedWorkspaceId = teamWorkspaceId;
            }
            else if (Request.Cookies.TryGetValue("SelectedWorkspaceId", out var savedWorkspaceId) &&
                     int.TryParse(savedWorkspaceId, out var cookieWorkspaceId))
            {
                selectedWorkspaceId = cookieWorkspaceId;
            }

            var model = new WorkspaceSwitcherViewModel
            {
                Workspaces = workspaces,
                SelectedWorkspaceId = selectedWorkspaceId
            };

            return View(model);
        }
    }
}