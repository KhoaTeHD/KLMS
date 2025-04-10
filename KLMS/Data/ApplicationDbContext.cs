using KLMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KLMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
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
