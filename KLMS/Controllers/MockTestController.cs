using KLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLMS.Controllers
{
    [Authorize]
    public class MockTestController : Controller
    {
        private readonly UserManager<User> _userManager;

        public MockTestController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: MockTest/Index
        public async Task<IActionResult> Index()
        {
            // Lấy thông tin user hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Tạo mock data cho demo
            var model = new MockTestViewModel
            {
                StudentInfo = new StudentTestInfo
                {
                    FullName = user?.FullName ?? "Nguyễn Thị Linh Chi",
                    StudentId = "01000071",
                    DateOfBirth = new DateTime(2002, 12, 9),
                    Gender = "Nữ",
                    Session = 8
                },
                Council = new TestCouncil
                {
                    CouncilName = "Sở GDDT Hà Nội",
                    TestLocation = "THPT Trấn Hưng Đạo",
                    Room = "Phòng 1"
                },
                Subjects = new List<TestSubject>
                {
                    new TestSubject { SubjectName = "Tiếng Trung Quốc", SubjectCode = "chinese", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Tiếng Nhật", SubjectCode = "japanese", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Toán", SubjectCode = "math", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Tiếng Anh", SubjectCode = "english", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Tiếng Nga", SubjectCode = "russian", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Tiếng Hàn", SubjectCode = "korean", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Lịch sử", SubjectCode = "history", IsActive = true, Url = "#" },
                    new TestSubject { SubjectName = "Tin học", SubjectCode = "informatics", IsActive = true, Url = "#" }
                }
            };

            return View(model);
        }

        // GET: MockTest/StartTest
        public IActionResult StartTest(string subjectCode)
        {
            // Logic để bắt đầu bài thi cho môn học cụ thể
            // Có thể redirect đến trang làm bài thi hoặc load nội dung bài thi

            ViewBag.SubjectCode = subjectCode;
            return View();
        }
    }
}