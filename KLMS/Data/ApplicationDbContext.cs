using KLMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KLMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        public DbSet<Class> Classes { get; set; }
        public DbSet<Lecture> Lectures { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null)
                {
                    if (tableName.StartsWith("AspNet"))
                    {
                        entityType.SetTableName(tableName.Substring(6));
                    }
                }
            }

            builder.Entity<Class>()
                .HasMany(c => c.Students)
                .WithMany(u => u.Classes)
                .UsingEntity(j => j.ToTable("ClassStudents"));

            // config for test entity
            builder.Entity<Test>(entity =>
            {
                entity.Property(e => e.TotalPoints).HasColumnType("decimal(5,2)");
                entity.Property(e => e.PassScore).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Point).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Test)
                    .WithMany(t => t.Questions)
                    .HasForeignKey(e => e.TestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TestAttempt>(entity =>
            {
                entity.Property(e => e.Score).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Test)
                    .WithMany(t => t.TestAttempts)
                    .HasForeignKey(e => e.TestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserAnswer>(entity =>
            {
                entity.HasOne(e => e.TestAttempt)
                    .WithMany(ta => ta.UserAnswers)
                    .HasForeignKey(e => e.TestAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public async Task SeedUsers(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "dangkhoa014@gmail.com",
                FullName = "Quản trị viên"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                var role = new IdentityRole("Admin");
                await roleManager.CreateAsync(role);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
