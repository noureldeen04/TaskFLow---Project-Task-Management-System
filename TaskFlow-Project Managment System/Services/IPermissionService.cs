using TaskFlow_Project_Managment_System.Models.Enums;

namespace TaskFlow_Project_Managment_System.Services
{
    public enum WorkspacePermission
    {
        ViewContent,
        CreateTask,
        EditTask,
        Comment,
        CreateProject,
        ManageProjectSettings,
        ArchiveProject,
        InviteMembers,
        ChangeRoles,
        RemoveMembers,
        ManageBilling,
        DeleteWorkspace
    }

    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, int workspaceId, WorkspacePermission permission);
    }
}