using Microsoft.EntityFrameworkCore;
using TaskFlow_Project_Managment_System.Models;
using TaskFlow_Project_Managment_System.Models.Entities;

namespace TaskFlow_Project_Managment_System.Context
{
    public class WorkFlowContext : DbContext
    {
        public WorkFlowContext(DbContextOptions<WorkFlowContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }

        public DbSet<WorkSpace> WorkSpaces { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<AllTask> Tasks { get; set; }

        public DbSet<ProjectPermissionSettings> ProjectPermissionSettings { get; set; }


        // Nour-Zoubaa Connection string
        // "Data Source=ZOUBAA\\SQLEXPRESS;Initial Catalog=TaskFlowFP;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=ZOUBAA\\SQLEXPRESS;Initial Catalog=TaskFlowFP;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email);
                e.Property(u=>u.FullName).IsRequired().HasMaxLength(100);
                e.Property(u=>u.Email).IsRequired().HasMaxLength(250);

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

                // Workspace Owner
                e.HasOne(w => w.Owner)
                    .WithMany(u => u.OwnedWorkspaces)
                    .HasForeignKey(w => w.OwnerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Project>(e =>
            {
                e.ToTable("Projects");
                e.HasKey(p =>  p.Id);
                e.Property(p => p.Name).IsRequired().HasMaxLength(100);
                e.Property(p => p.Description).HasMaxLength(1000);

                //workspace FK
                e.HasOne(p => p.workSpaces)
                .WithMany(p => p.Projects)
                .HasForeignKey(p => p.WorkspaceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

                //Created by user FK
                e.HasOne(p => p.Users)
                .WithMany( p => p.CreatedProjects)
                .HasForeignKey(p => p.CreatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.PermissionSettings)
                    .WithOne(settings => settings.Project)
                    .HasForeignKey<ProjectPermissionSettings>(settings => settings.ProjectId)
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
                e.Property(t => t.Title).IsRequired().HasMaxLength(200);
                e.Property(t => t.Description).HasMaxLength(4000);


                //relation with project 
                e.HasOne(t => t.projects)
                .WithMany(t => t.ProjectTasks)
                .HasForeignKey(t => t.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);



                //relations with user table 
                        //Creation
                e.HasOne(t => t.CreatedByUser)
                .WithMany(t => t.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                //Assign

                e.HasOne(t => t.AssignedByUser)
               .WithMany(t => t.AssignedTasks)
               .HasForeignKey(t => t.AssignedToUserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
            });
           


        }

    }
}
