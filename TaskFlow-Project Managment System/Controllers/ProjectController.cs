using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.Entities;
using TaskFlow_Project_Managment_System.Models.Enums;
using TaskFlow_Project_Managment_System.Models.ViewModels;

namespace TaskFlow_Project_Managment_System.Controllers
{
    public class ProjectController : Controller
    {
        private readonly WorkFlowContext _context;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(WorkFlowContext context, ILogger<ProjectController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _context.Projects.AsNoTracking()
                    .OrderByDescending(project => project.CreatedAt)
                    .Select(project => new ProjectListItemViewModel
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        Template = project.Template,
                        CreatedAt = project.CreatedAt
                    })
                    .ToListAsync();

                return View(projects);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to load projects.");
                ViewData["LoadError"] = "Projects could not be loaded. Check the database connection and try again.";
                return View(Array.Empty<ProjectListItemViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProjectCreateViewModel();
            await PopulateChoicesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            if (!Enum.IsDefined(model.Template))
            {
                ModelState.AddModelError(nameof(model.Template), "Choose one of the available project templates.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var workspaceExists = await _context.WorkSpaces.AnyAsync(workspace => workspace.Id == model.WorkspaceId);
                    var userExists = await _context.Users.AnyAsync(user => user.Id == model.CreatedByUserId);
                    if (!workspaceExists)
                        ModelState.AddModelError(nameof(model.WorkspaceId), "Choose an available workspace.");
                    if (!userExists)
                        ModelState.AddModelError(nameof(model.CreatedByUserId), "Choose an available project creator.");
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to validate project workspace and creator selections.");
                    ModelState.AddModelError(string.Empty, "Project details could not be verified. Check the database connection and try again.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateChoicesAsync(model);
                return View(model);
            }

            var project = new Project
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                Template = model.Template,
                CreatedAt = DateTime.UtcNow,
                WorkspaceId = model.WorkspaceId,
                CreatedByUserId = model.CreatedByUserId,
                PermissionSettings = new ProjectPermissionSettings()
            };

            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (DbUpdateException exception)
            {
                _logger.LogError(exception, "Unable to save project {ProjectName}.", project.Name);
                ModelState.AddModelError(string.Empty, "The project could not be saved. Please check the database connection and try again.");
                await PopulateChoicesAsync(model);
                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while saving project {ProjectName}.", project.Name);
                ModelState.AddModelError(string.Empty, "An unexpected database error prevented the project from being saved. Please try again.");
                await PopulateChoicesAsync(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects.AsNoTracking()
                .Where(item => item.Id == id)
                .Select(item => new ProjectDetailsViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Template = item.Template,
                    CreatedAt = item.CreatedAt,
                    WorkspaceName = item.workSpaces.Name,
                    CreatorName = item.Users.FullName
                })
                .FirstOrDefaultAsync();

            if (project is null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Settings(int id)
        {
            var project = await _context.Projects.AsNoTracking()
                .Where(item => item.Id == id)
                .Select(item => new ProjectSettingsViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    WorkspaceId = item.WorkspaceId
                })
                .FirstOrDefaultAsync();

            if (project is null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            await PopulateWorkspaceChoicesAsync(project);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(int id, ProjectSettingsViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var project = await _context.Projects.FirstOrDefaultAsync(item => item.Id == id);
                    if (project is null) return NotFound();
                    var workspaceExists = await _context.WorkSpaces.AnyAsync(workspace => workspace.Id == model.WorkspaceId);
                    if (!workspaceExists)
                    {
                        ModelState.AddModelError(nameof(model.WorkspaceId), "Choose an available workspace.");
                    }
                    else
                    {
                        project.Name = model.Name.Trim();
                        project.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                        project.WorkspaceId = model.WorkspaceId;
                        await _context.SaveChangesAsync();
                        TempData["ProjectSettingsSuccess"] = "Project settings were saved.";
                        return RedirectToAction(nameof(Settings), new { id });
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to update settings for project {ProjectId}.", id);
                    ModelState.AddModelError(string.Empty, "Project settings could not be saved. Check the database connection and try again.");
                }
            }

            model.Id = id;
            ViewData["ProjectId"] = id;
            await PopulateWorkspaceChoicesAsync(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Permissions(int id, bool edit = false)
        {
            var model = await LoadPermissionsAsync(id);
            if (model is null) return NotFound();

            model.IsEditing = edit;
            ViewData["ProjectId"] = id;
            ViewData["Title"] = $"{model.ProjectName} permissions";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(int id, ProjectPermissionsViewModel model)
        {
            if (id != model.ProjectId) return BadRequest();
            if (!Enum.IsDefined(model.ViewAccess))
                ModelState.AddModelError(nameof(model.ViewAccess), "Choose a supported view access setting.");
            if (!Enum.IsDefined(model.EditAccess))
                ModelState.AddModelError(nameof(model.EditAccess), "Choose a supported edit access setting.");

            if (ModelState.IsValid)
            {
                try
                {
                    var project = await _context.Projects.Include(item => item.PermissionSettings)
                        .FirstOrDefaultAsync(item => item.Id == id);
                    if (project is null) return NotFound();

                    var settings = project.PermissionSettings ?? new ProjectPermissionSettings { ProjectId = id };
                    settings.ViewAccess = model.ViewAccess;
                    settings.EditAccess = model.EditAccess;
                    settings.UpdatedAt = DateTime.UtcNow;
                    if (project.PermissionSettings is null)
                        _context.ProjectPermissionSettings.Add(settings);

                    await _context.SaveChangesAsync();
                    TempData["ProjectPermissionsSuccess"] = "Permission configuration was saved.";
                    return RedirectToAction(nameof(Permissions), new { id });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to save permission configuration for project {ProjectId}.", id);
                    ModelState.AddModelError(string.Empty, "Permission configuration could not be saved. Check the database connection and try again.");
                }
            }

            var projectContext = await LoadPermissionsAsync(id);
            if (projectContext is null) return NotFound();
            model.ProjectName = projectContext.ProjectName;
            model.IsEditing = true;
            model.IsPersisted = projectContext.IsPersisted;
            ViewData["ProjectId"] = id;
            ViewData["Title"] = $"{model.ProjectName} permissions";
            return View(model);
        }

        private async Task<ProjectPermissionsViewModel?> LoadPermissionsAsync(int id)
        {
            return await _context.Projects.AsNoTracking()
                .Where(project => project.Id == id)
                .Select(project => new ProjectPermissionsViewModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    IsPersisted = project.PermissionSettings != null,
                    ViewAccess = project.PermissionSettings == null
                        ? ProjectAccessLevel.WorkspaceMembers
                        : project.PermissionSettings.ViewAccess,
                    EditAccess = project.PermissionSettings == null
                        ? ProjectAccessLevel.ProjectCreatorOnly
                        : project.PermissionSettings.EditAccess
                })
                .FirstOrDefaultAsync();
        }

        private async Task PopulateWorkspaceChoicesAsync(ProjectSettingsViewModel model)
        {
            try
            {
                model.Workspaces = await _context.WorkSpaces.AsNoTracking()
                    .OrderBy(workspace => workspace.Name)
                    .Select(workspace => new SelectListItem(workspace.Name, workspace.Id.ToString()))
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to load workspace choices for project {ProjectId}.", model.Id);
                ModelState.AddModelError(string.Empty, "Workspace options could not be loaded. Check the database connection and try again.");
                model.Workspaces = [];
            }
        }

        private async Task PopulateChoicesAsync(ProjectCreateViewModel model)
        {
            try
            {
                model.Workspaces = await _context.WorkSpaces.AsNoTracking()
                    .OrderBy(workspace => workspace.Name)
                    .Select(workspace => new SelectListItem(workspace.Name, workspace.Id.ToString()))
                    .ToListAsync();
                model.Users = await _context.Users.AsNoTracking()
                    .OrderBy(user => user.FullName)
                    .Select(user => new SelectListItem($"{user.FullName} ({user.Email})", user.Id.ToString()))
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to load project creation choices.");
                ModelState.AddModelError(string.Empty, "Workspace and creator options could not be loaded. Check the database connection and try again.");
                model.Workspaces = [];
                model.Users = [];
            }
        }
    }
}
