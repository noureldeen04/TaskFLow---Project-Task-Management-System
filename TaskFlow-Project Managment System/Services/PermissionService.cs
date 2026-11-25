using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly WorkFlowContext _db;

        public PermissionService(WorkFlowContext db)
        {
            _db = db;
        }

        private static readonly Dictionary<WorkspacePermission, WorkspaceRole> MinimumRole = new()
        {
            [WorkspacePermission.ViewContent] = WorkspaceRole.Viewer,
            [WorkspacePermission.CreateTask] = WorkspaceRole.Member,
            [WorkspacePermission.EditTask] = WorkspaceRole.Member,
            [WorkspacePermission.Comment] = WorkspaceRole.Member,
            [WorkspacePermission.CreateProject] = WorkspaceRole.Admin,
            [WorkspacePermission.ManageProjectSettings] = WorkspaceRole.Admin,
            [WorkspacePermission.ArchiveProject] = WorkspaceRole.Admin,
            [WorkspacePermission.InviteMembers] = WorkspaceRole.Admin,
            [WorkspacePermission.RemoveMembers] = WorkspaceRole.Admin,
            [WorkspacePermission.ChangeRoles] = WorkspaceRole.Admin,
            [WorkspacePermission.ManageBilling] = WorkspaceRole.Owner,
            [WorkspacePermission.DeleteWorkspace] = WorkspaceRole.Owner,
        };

        public async Task<bool> HasPermissionAsync(int userId, int workspaceId, WorkspacePermission permission)
        {
            var role = await _db.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
                .Select(m => m.Role)
                .FirstOrDefaultAsync();

            if (role == default) return false;

            return role >= MinimumRole[permission];
        }
    }
}