using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskFlow_Project_Managment_System.Context;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.Entities;
using TaskFlow_Project_Managment_System.Models.Enums;
using Microsoft.EntityFrameworkCore;


namespace TaskFlow_Project_Managment_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly WorkFlowContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public AccountController(WorkFlowContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email
            };
            user.PasswordHash = _hasher.HashPassword(user, model.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await EnsureWorkspaceMembershipAsync(user);

            await SignInAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            await EnsureWorkspaceMembershipAsync(user);
            await SignInAsync(user);
            return RedirectToAction("Index", "Home");
        }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync("TaskFlowCookie");
    return RedirectToAction("Login");
}

        private async Task SignInAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "TaskFlowCookie");
            await HttpContext.SignInAsync("TaskFlowCookie", new ClaimsPrincipal(identity));
        }

        private async Task EnsureWorkspaceMembershipAsync(User user)
        {
            if (!user.WorkspaceId.HasValue)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                var workspaceName = $"{user.FullName}'s Workspace";
                if (workspaceName.Length > 100)
                    workspaceName = workspaceName[..100];

                var workspace = new WorkSpace
                {
                    Name = workspaceName,
                    OwnerId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _db.WorkSpaces.Add(workspace);
                await _db.SaveChangesAsync();

                user.WorkspaceId = workspace.Id;
                _db.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = workspace.Id,
                    UserId = user.Id,
                    Role = WorkspaceRole.Owner
                });
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return;
            }

            var hasMembership = await _db.WorkspaceMembers.AnyAsync(member =>
                member.WorkspaceId == user.WorkspaceId.Value && member.UserId == user.Id);
            if (hasMembership) return;

            var isOwner = await _db.WorkSpaces.AnyAsync(workspace =>
                workspace.Id == user.WorkspaceId.Value && workspace.OwnerId == user.Id);

            _db.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = user.WorkspaceId.Value,
                UserId = user.Id,
                Role = isOwner ? WorkspaceRole.Owner : WorkspaceRole.Member
            });
            await _db.SaveChangesAsync();
        }
    }
}
