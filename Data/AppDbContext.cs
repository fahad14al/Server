using Microsoft.EntityFrameworkCore;
namespace Server.Data

{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Server.Models.User> Users { get; set; }
        public DbSet<Server.Models.Project> Projects { get; set; }
        public DbSet<Server.Models.TaskItem> TaskItems { get; set; }
        public DbSet<Server.Models.Comment> Comments { get; set; }
        public DbSet<Server.Models.ProjectMember> ProjectMembers { get; set; }
        public DbSet<Server.Models.TaskAssignment> TaskAssignments { get; set; }
        public DbSet<Server.Models.Role> Roles { get; set; }
        public DbSet<Server.Models.UserRole> UserRoles { get; set; }


        // Configure the relationships using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //user
            modelBuilder.Entity<Server.Models.User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.UserEmail)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(u => u.UserEmail)
                    .IsUnique();
            });

            //project

            modelBuilder.Entity<Server.Models.Project>(entity =>
            {
                entity.HasKey(p => p.ProjectId);

                entity.Property(p => p.ProjectName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(p => p.ProjectDescription)
                    .HasMaxLength(1000);
                // Configure the relationship between Project and User (Creator)
                entity.HasOne(p => p.Creator)
                    .WithMany(u => u.Projects)
                    .HasForeignKey(p => p.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);




            });

            //task
            modelBuilder.Entity<Server.Models.TaskItem>(entity =>
            {
                entity.HasKey(t => t.TaskItemId);
                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(t => t.Description)
                    .HasMaxLength(1000);
                // Configure the relationship between TaskItem and Project
                entity.HasOne(t => t.Project)
                    .WithMany(p => p.TaskItems)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Configure the relationship between TaskItem and User (Creator)
                entity.HasOne(t => t.Creator)
                    .WithMany(u => u.TaskItems)
                    .HasForeignKey(t => t.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //comment
            modelBuilder.Entity<Server.Models.Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);
                entity.Property(c => c.Content)
                    .IsRequired()
                    .HasMaxLength(1000);
                // Configure the relationship between Comment and TaskItem
                entity.HasOne(c => c.TaskItem)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Configure the relationship between Comment and User
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //projectmember
            modelBuilder.Entity<Server.Models.ProjectMember>(entity =>
            {
                entity.HasKey(pm => new { pm.ProjectId, pm.UserId });
                // Configure the relationship between ProjectMember and Project
                entity.HasOne(pm => pm.Project)
                    .WithMany(p => p.ProjectMembers)
                    .HasForeignKey(pm => pm.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Configure the relationship between ProjectMember and User
                entity.HasOne(pm => pm.User)
                    .WithMany(u => u.ProjectMembers)
                    .HasForeignKey(pm => pm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //taskassignment
            modelBuilder.Entity<Server.Models.TaskAssignment>(entity =>
            {
                entity.HasKey(ta => new { ta.UserId, ta.TaskItemId });
                // Configure the relationship between TaskAssignment and User
                entity.HasOne(ta => ta.User)
                    .WithMany(u => u.Assignments)
                    .HasForeignKey(ta => ta.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Configure the relationship between TaskAssignment and TaskItem
                entity.HasOne(ta => ta.TaskItem)
                    .WithMany(t => t.TaskAssignments)
                    .HasForeignKey(ta => ta.TaskItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Role
            modelBuilder.Entity<Server.Models.Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            //userrole
            modelBuilder.Entity<Server.Models.UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                // Configure the relationship between UserRole and User
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Configure the relationship between UserRole and Role
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
