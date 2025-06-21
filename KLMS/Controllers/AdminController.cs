using KLMS.Data;
using KLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: Admin (Default action - redirect to Dashboard)
        public async Task<IActionResult> Index()
        {
            return await Dashboard();
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var totalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;
                var totalTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).Count;
                var totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                var totalClasses = await _context.Classes.CountAsync();

                var recentUsers = await _userManager.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var model = new AdminStatisticsViewModel
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalAdmins = totalAdmins,
                    TotalClasses = totalClasses,
                    RecentUsers = recentUsers
                };

                _logger.LogInformation("Admin dashboard loaded successfully");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải bảng điều khiển. Vui lòng thử lại.";
                return View(new AdminStatisticsViewModel());
            }
        }

        // GET: Admin/UserManagement
        public async Task<IActionResult> UserManagement(string searchTerm = "", string roleFilter = "", int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsQueryable();

            // Search by name or email
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            // Filter by role
            if (!string.IsNullOrEmpty(roleFilter))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIds.Contains(u.Id));
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get roles for each user
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            var model = new UserManagementViewModel
            {
                Users = users,
                UserRoles = userRoles,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                CurrentPage = page,
                PageSize = pageSize,
                TotalUsers = totalUsers,
                TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
            };

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // GET: Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(new CreateUserViewModel());
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // Auto confirm for admin created accounts
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to selected role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    _logger.LogInformation($"Admin created new user: {user.Email} with role: {model.Role}");
                    TempData["SuccessMessage"] = $"Tài khoản {user.Email} đã được tạo thành công!";
                    return RedirectToAction(nameof(UserManagement));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                CurrentRole = userRoles.FirstOrDefault(),
                IsEmailConfirmed = user.EmailConfirmed
            };

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.EmailConfirmed = model.IsEmailConfirmed;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update user role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    if (!string.IsNullOrEmpty(model.NewRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.NewRole);
                    }

                    _logger.LogInformation($"Admin updated user: {user.Email}");
                    TempData["SuccessMessage"] = $"Tài khoản {user.Email} đã được cập nhật thành công!";
                    return RedirectToAction(nameof(UserManagement));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            // Prevent deleting admin users
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản quản trị viên." });
            }

            // Check if user is teaching any classes
            var isTeaching = await _context.Classes.AnyAsync(c => c.TeacherId == user.Id);
            if (isTeaching)
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản đang phụ trách lớp học." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Admin deleted user: {user.Email}");
                return Json(new { success = true, message = "Xóa tài khoản thành công." });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài khoản." });
        }

        // POST: Admin/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Admin reset password for user: {user.Email}");
                return Json(new { success = true, message = "Đặt lại mật khẩu thành công." });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = $"Lỗi: {errors}" });
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;
            var totalTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).Count;
            var totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var totalClasses = await _context.Classes.CountAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new AdminStatisticsViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalAdmins = totalAdmins,
                TotalClasses = totalClasses,
                RecentUsers = recentUsers
            };

            return View(model);
        }

        // GET: Admin/SystemHealth
        [HttpGet]
        public IActionResult GetSystemHealth()
        {
            try
            {
                // Simulate system health check
                var healthData = new
                {
                    cpu = 25,
                    memory = 68,
                    disk = 75,
                    network = "Good",
                    uptime = "99.9%",
                    status = "Healthy"
                };

                return Json(healthData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking system health");
                return Json(new { status = "Error", message = "Unable to check system health" });
            }
        }

        // POST: Admin/BulkCreateUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreateUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file." });
            }

            try
            {
                // TODO: Implement Excel file processing for bulk user creation
                // This is a placeholder for future implementation

                _logger.LogInformation("Bulk user creation attempted");
                return Json(new { success = false, message = "Chức năng tạo hàng loạt đang được phát triển." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk user creation");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý file." });
            }
        }

        // GET: Admin/ExportUsers
        public async Task<IActionResult> ExportUsers()
        {
            try
            {
                // TODO: Implement user data export functionality
                // This is a placeholder for future implementation

                _logger.LogInformation("User data export requested");
                TempData["InfoMessage"] = "Chức năng xuất dữ liệu đang được phát triển.";
                return RedirectToAction(nameof(UserManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting user data");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất dữ liệu.";
                return RedirectToAction(nameof(UserManagement));
            }
        }
    }

    // ViewModels (keep existing ones and add any missing)
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; } = new();
        public Dictionary<string, IList<string>> UserRoles { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string RoleFilter { get; set; } = "";
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Vai trò hiện tại")]
        public string CurrentRole { get; set; }

        [Display(Name = "Vai trò mới")]
        public string NewRole { get; set; }

        [Display(Name = "Email đã xác thực")]
        public bool IsEmailConfirmed { get; set; }
    }

    public class AdminStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalClasses { get; set; }
        public List<User> RecentUsers { get; set; } = new();
    }
}