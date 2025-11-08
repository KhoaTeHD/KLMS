using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KLMS.Data;
using KLMS.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;

namespace KLMS.Controllers
{
    [Authorize]
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClassController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Class
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IQueryable<Class> classesQuery = _context.Classes.Include(c => c.Teacher);


            // Admin: Hiển thị tất cả lớp học
            if (User.IsInRole("Admin"))
            {
                // Không cần lọc
            }
            // Teacher: Chỉ hiển thị lớp mà giáo viên này dạy
            else if (User.IsInRole("Teacher"))
            {
                classesQuery = classesQuery.Where(c => c.TeacherId == userId);
            }
            // Student: Chỉ hiển thị lớp mà học sinh này tham gia
            else if (User.IsInRole("Student"))
            {
                classesQuery = classesQuery
                    .Where(c => c.Students.Any(cs => cs.Id == userId));
            }

            return View(await classesQuery.ToListAsync());
        }

        // GET: Classes/ClassView/5
        public async Task<IActionResult> ClassView(long id, string tab = "lectures")
        {
            // Kiểm tra quyền truy cập
            var userId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var classItem = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .Include(c => c.Lectures)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classItem == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu người dùng không phải admin hoặc giáo viên phụ trách,
            // và không phải học sinh trong lớp thì không cho truy cập
            if (!userRoles.Contains("Admin") && classItem.TeacherId != userId && !classItem.Students.Any(s => s.Id == userId))
            {
                return Forbid();
            }

            // Truyền role người dùng vào ViewBag
            ViewBag.UserRole = userRoles.FirstOrDefault();
            ViewBag.ActiveTab = tab;

            return View(classItem);
        }

        // GET: Class/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Class/Create
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Create()
        {
            var teachers = _userManager.GetUsersInRoleAsync("Teacher").Result;
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Class/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create([Bind("Id,ClassName, Description,TeacherId")] Class @class)
        {
            if (ModelState.IsValid)
            {
                @class.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
                @class.LastModified = DateTime.Now;

                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "FullName", @class.TeacherId);
            return View(@class);
        }

        // GET: Class/Edit/5
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền: Teacher chỉ có thể edit lớp của mình
            if (User.IsInRole("Teacher"))
            {
                var userId = _userManager.GetUserId(User);
                if (@class.TeacherId != userId)
                {
                    return Forbid();
                }
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewData["TeacherId"] = new SelectList(teachers, "Id", "FullName", @class.TeacherId);
            return View(@class);
        }

        // POST: Class/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ClassName,TeacherId")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    @class.LastModified = DateTime.Now;
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "Id", @class.TeacherId);
            return View(@class);
        }

        // GET: Class/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Class/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Thêm bài giảng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> AddLecture(long classId, string title, string description, IFormFile lectureFile)
        {
            var classItem = await _context.Classes.FindAsync(classId);
            if (classItem == null) return NotFound();

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && classItem.TeacherId != userId)
            {
                return Forbid();
            }

            var lecture = new Lecture
            {
                Title = title,
                Description = description,
                ClassId = classId,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now
            };

            // Xử lý upload file
            if (lectureFile != null && lectureFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + lectureFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await lectureFile.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn relative
                lecture.FilePath = "/uploads/lectures/" + uniqueFileName;
            }

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClassView", new { id = classId, tab = "lectures" });
        }

        // GET: Lấy nội dung bài giảng
        [HttpGet]
        public async Task<IActionResult> GetLectureContent(long lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Class)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            var userId = _userManager.GetUserId(User);
            var userRoles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));

            if (!userRoles.Contains("Admin") &&
                lecture.Class.TeacherId != userId &&
                !lecture.Class.Students.Any(s => s.Id == userId))
            {
                return Forbid();
            }

            var lectureData = new
            {
                id = lecture.Id,
                title = lecture.Title,
                description = lecture.Description,
                createdDate = lecture.CreatedDate.ToString("dd/MM/yyyy"),
                lastModified = lecture.LastModified.ToString("dd/MM/yyyy HH:mm"),
                filePath = lecture.FilePath,
                classId = lecture.ClassId
            };

            return Json(lectureData);
        }

        // GET: Lấy thông tin bài giảng để edit
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetLecture(long lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Class)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null) return NotFound();

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && lecture.Class.TeacherId != userId)
            {
                return Forbid();
            }

            var lectureData = new
            {
                id = lecture.Id,
                title = lecture.Title,
                description = lecture.Description,
                classId = lecture.ClassId
            };

            return Json(lectureData);
        }

        // POST: Chỉnh sửa bài giảng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> EditLecture(long id, string title, string description, long classId, IFormFile lectureFile)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Class)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null) return NotFound();

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && lecture.Class.TeacherId != userId)
            {
                return Forbid();
            }

            lecture.Title = title;
            lecture.Description = description;
            lecture.LastModified = DateTime.Now;

            // Xử lý upload file mới nếu có
            if (lectureFile != null && lectureFile.Length > 0)
            {
                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(lecture.FilePath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Upload file mới
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + lectureFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await lectureFile.CopyToAsync(fileStream);
                }

                lecture.FilePath = "/uploads/lectures/" + uniqueFileName;
            }

            _context.Update(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClassView", new { id = classId, tab = "lectures" });
        }

        // POST: Xóa bài giảng
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteLecture(long lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Class)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài giảng." });
            }

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && lecture.Class.TeacherId != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bài giảng này." });
            }

            // Xóa file nếu có
            if (!string.IsNullOrEmpty(lecture.FilePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa bài giảng thành công." });
        }

        // GET: Tìm kiếm học sinh để thêm vào lớp
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> SearchStudents(string query, long classId)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            var classItem = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classItem == null) return Json(new List<object>());

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && classItem.TeacherId != userId)
            {
                return Json(new List<object>());
            }

            // Lấy danh sách học sinh chưa có trong lớp
            var studentsInClass = classItem.Students.Select(s => s.Id).ToList();
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var searchResults = students
                .Where(s => !studentsInClass.Contains(s.Id) &&
                           (s.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(10)
                .Select(s => new
                {
                    id = s.Id,
                    fullName = s.FullName,
                    email = s.Email
                })
                .ToList();

            return Json(searchResults);
        }

        // POST: Thêm học sinh vào lớp
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> AddStudentToClass(string studentId, long classId)
        {
            var classItem = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học." });
            }

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && classItem.TeacherId != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền thêm học sinh vào lớp này." });
            }

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null)
            {
                return Json(new { success = false, message = "Không tìm thấy học sinh." });
            }

            // Kiểm tra học sinh đã có trong lớp chưa
            if (classItem.Students.Any(s => s.Id == studentId))
            {
                return Json(new { success = false, message = "Học sinh đã có trong lớp học này." });
            }

            classItem.Students.Add(student);
            classItem.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm học sinh vào lớp thành công." });
        }

        // POST: Xóa học sinh khỏi lớp
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> RemoveStudentFromClass(string studentId, long classId)
        {
            var classItem = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học." });
            }

            // Kiểm tra quyền
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && classItem.TeacherId != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa học sinh khỏi lớp này." });
            }

            var student = classItem.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return Json(new { success = false, message = "Học sinh không có trong lớp học này." });
            }

            classItem.Students.Remove(student);
            classItem.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa học sinh khỏi lớp thành công." });
        }

        private bool ClassExists(long id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}
