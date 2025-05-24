using KLMS.Models;
using Microsoft.AspNetCore.Identity;

namespace KLMS.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            string[] roleNames = { "Admin", "Teacher", "Student" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo tài khoản admin nếu chưa tồn tại
            //string adminUserName = "admin"; // thay đổi tên tài khoản cho phù hợp
            string adminEmail = "dangkhoa014@gmail.com"; // thay đổi email cho phù hợp
            string adminPassword = "admin@123"; // thay đổi mật khẩu theo tiêu chuẩn bảo mật

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception("Không thể tạo tài khoản admin: " + string.Join(", ", result.Errors));
                }
            }

            //string teacherUserName = "admin"; // thay đổi tên tài khoản cho phù hợp
            string teacherEmail = "dangkhoa013@gmail.com"; // thay đổi email cho phù hợp
            string teacherPassword = "teacher@123"; // thay đổi mật khẩu theo tiêu chuẩn bảo mật

            var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
            if (teacherUser == null)
            {
                teacherUser = new User
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FullName = "Ngô Bảo Ngọc"
                };

                var result = await userManager.CreateAsync(teacherUser, teacherPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
                }
                else
                {
                    throw new Exception("Không thể tạo tài khoản teacher: " + string.Join(", ", result.Errors));
                }
            }
        }
    }
}
