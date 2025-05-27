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

namespace KLMS.Controllers
{
    [Authorize]
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ClassController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Class
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            IQueryable<Class> classesQuery = _context.Classes.Include(c => c.Teacher);

            //var applicationDbContext = _context.Classes.Include(c => c.Teacher);

            // Admin: Hiển thị tất cả lớp học
            if (User.IsInRole("Administrator"))
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
        public IActionResult Create()
        {
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Class/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassName, Description,TeacherId")] Class @class)
        {
            if (ModelState.IsValid)
            {
                // Gán các giá trị mặc định nếu cần
                @class.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
                @class.LastModified = DateTime.Now;

                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "Id", @class.TeacherId);
            return View(@class);
        }

        // GET: Class/Edit/5
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
            ViewData["TeacherId"] = new SelectList(_context.Users, "Id", "Id", @class.TeacherId);
            return View(@class);
        }

        // POST: Class/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        private bool ClassExists(long id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}
