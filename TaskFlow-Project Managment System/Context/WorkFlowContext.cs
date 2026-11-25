using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.Entities;

namespace TaskFlow_Project_Managment_System.Context
{
    public class WorkFlowContext : DbContext
    {
        public WorkFlowContext(DbContextOptions<WorkFlowContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<WorkSpace> WorkSpaces { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<WorkspaceInvitation> WorkspaceInvitations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AllTask> Tasks { get; set; }
        public DbSet<ProjectPermissionSettings> ProjectPermissionSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email);
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                e.Property(u => u.Email).IsRequired().HasMaxLength(250);

                e.HasOne(u => u.workSpaces)
                    .WithMany(w => w.Members)
                    .HasForeignKey(u => u.WorkspaceId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkSpace>(e =>
            {
                e.ToTable("Workspaces");
                e.HasKey(w => w.Id);

                e.Property(w => w.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(w => w.Description)
                    .HasMaxLength(500);

                e.HasOne(w => w.Owner)
                    .WithMany(u => u.OwnedWorkspaces)
                    .HasForeignKey(w => w.OwnerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Team>(e =>
            {
                e.ToTable("Teams");
                e.HasKey(t => t.Id);

                e.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(t => t.Description)
                    .HasMaxLength(500);

                e.HasOne(t => t.WorkSpace)
                    .WithMany(w => w.Teams)
                    .HasForeignKey(t => t.WorkspaceId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ActivityLog>(e =>
            {
                e.ToTable("ActivityLogs");
                e.HasKey(activity => activity.Id);

                e.Property(activity => activity.Action)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(activity => activity.Description)
                    .HasMaxLength(500);

                e.HasOne(activity => activity.WorkSpace)
                    .WithMany(workspace => workspace.ActivityLogs)
                    .HasForeignKey(activity => activity.WorkspaceId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(activity => activity.User)
                    .WithMany()
                    .HasForeignKey(activity => activity.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkspaceInvitation>(e =>
            {
                e.ToTable("WorkspaceInvitations");
                e.HasKey(invitation => invitation.Id);

                e.Property(invitation => invitation.Email)
                    .IsRequired()
                    .HasMaxLength(250);

                e.Property(invitation => invitation.Status)
                    .IsRequired()
                    .HasMaxLength(30);

                e.HasOne(invitation => invitation.WorkSpace)
                    .WithMany(workspace => workspace.Invitations)
                    .HasForeignKey(invitation => invitation.WorkspaceId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(invitation => invitation.InvitedByUser)
                    .WithMany()
                    .HasForeignKey(invitation => invitation.InvitedByUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Project>(e =>
            {
                e.ToTable("Projects");
                e.HasKey(p => p.Id);

                e.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(p => p.Description)
                    .HasMaxLength(1000);

                e.HasOne(p => p.workSpaces)
                    .WithMany(workspace => workspace.Projects)
                    .HasForeignKey(p => p.WorkspaceId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.Users)
                    .WithMany(user => user.CreatedProjects)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.PermissionSettings)
                    .WithOne(settings => settings.Project)
                    .HasForeignKey<ProjectPermissionSettings>(
                        settings => settings.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProjectPermissionSettings>(e =>
            {
                e.ToTable("ProjectPermissionSettings");
                e.HasKey(settings => settings.ProjectId);

                e.Property(settings => settings.ViewAccess).IsRequired();
                e.Property(settings => settings.EditAccess).IsRequired();
                e.Property(settings => settings.UpdatedAt).IsRequired();
            });

            modelBuilder.Entity<AllTask>(e =>
            {
                e.ToTable("Tasks");
                e.HasKey(t => t.Id);

                e.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(t => t.Description)
                    .HasMaxLength(4000);

                e.HasOne(t => t.projects)
                    .WithMany(project => project.ProjectTasks)
                    .HasForeignKey(t => t.ProjectId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(t => t.CreatedByUser)
                    .WithMany(user => user.CreatedTasks)
                    .HasForeignKey(t => t.CreatedByUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.AssignedByUser)
                    .WithMany(user => user.AssignedTasks)
                    .HasForeignKey(t => t.AssignedToUserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}