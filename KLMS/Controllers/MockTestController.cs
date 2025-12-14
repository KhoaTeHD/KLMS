using KLMS.Models;
using KLMS.Data;
using KLMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KLMS.Controllers
{
    [Authorize]
    public class MockTestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MockTestController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MockTest/Index - Danh sách môn thi
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Lấy danh sách tests đang active
            var activeTests = await _context.Tests
                .Where(t => t.IsActive)
                .Select(t => new TestSubject
                {
                    SubjectName = GetSubjectName(t.SubjectCode),
                    SubjectCode = t.SubjectCode,
                    IsActive = true,
                    Url = Url.Action("StartTest", "MockTest", new { testId = t.Id }),
                    TestId = t.Id
                })
                .ToListAsync();

            var model = new MockTestViewModel
            {
                StudentInfo = new StudentTestInfo
                {
                    FullName = user?.FullName ?? "Học sinh",
                    StudentId = user?.UserName ?? "N/A",
                    DateOfBirth = DateTime.Now.AddYears(-18),
                    Gender = "N/A",
                    Session = 8
                },
                Council = new TestCouncil
                {
                    CouncilName = "Sở GDDT Hà Nội",
                    TestLocation = "THPT Trấn Hưng Đạo",
                    Room = "Phòng 1"
                },
                Subjects = activeTests
            };

            return View(model);
        }

        // GET: MockTest/StartTest/{testId} - Bắt đầu hoặc resume bài thi
        public async Task<IActionResult> StartTest(long testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null || !test.IsActive)
            {
                TempData["ErrorMessage"] = "Đề thi không tồn tại hoặc đã bị vô hiệu hóa.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra các lượt thi InProgress
            var inProgressAttempts = await _context.TestAttempts
                .Where(ta => ta.UserId == userId && ta.TestId == testId && ta.Status == AttemptStatus.InProgress)
                .OrderByDescending(ta => ta.StartTime)
                .ToListAsync();

            TestAttempt currentAttempt = null;

            if (inProgressAttempts.Any())
            {
                var latestAttempt = inProgressAttempts.First();

                // Tính thời gian còn lại
                var elapsed = (DateTime.Now - latestAttempt.StartTime).TotalSeconds;
                var remaining = (test.Duration * 60) - elapsed;

                if (remaining > 0)
                {
                    // Resume bài cũ
                    currentAttempt = latestAttempt;
                }
                else
                {
                    // Hết giờ - auto submit tất cả bài cũ
                    foreach (var attempt in inProgressAttempts)
                    {
                        await AutoSubmitAttempt(attempt.Id);
                    }
                }
            }

            // Nếu không có bài để resume, tạo bài mới
            if (currentAttempt == null)
            {
                // Kiểm tra số lần thi
                if (!test.AllowMultipleAttempts)
                {
                    var completedCount = await _context.TestAttempts
                        .CountAsync(ta => ta.UserId == userId && ta.TestId == testId && ta.Status == AttemptStatus.Completed);

                    if (completedCount > 0)
                    {
                        TempData["ErrorMessage"] = "Bạn đã hoàn thành bài thi này. Không được phép thi lại.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                if (test.MaxAttempts.HasValue)
                {
                    var attemptCount = await _context.TestAttempts
                        .CountAsync(ta => ta.UserId == userId && ta.TestId == testId);

                    if (attemptCount >= test.MaxAttempts.Value)
                    {
                        TempData["ErrorMessage"] = $"Bạn đã vượt quá số lần thi cho phép ({test.MaxAttempts.Value} lần).";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Tạo TestAttempt mới
                var attemptNumber = await _context.TestAttempts
                    .Where(ta => ta.UserId == userId && ta.TestId == testId)
                    .CountAsync() + 1;

                currentAttempt = new TestAttempt
                {
                    TestId = testId,
                    UserId = userId,
                    StartTime = DateTime.Now,
                    TimeRemaining = test.Duration * 60,
                    Status = AttemptStatus.InProgress,
                    AttemptNumber = attemptNumber,
                    LastSavedAt = DateTime.Now
                };

                _context.TestAttempts.Add(currentAttempt);
                await _context.SaveChangesAsync();

                // Tạo UserAnswer records
                foreach (var question in test.Questions)
                {
                    var userAnswer = new UserAnswer
                    {
                        TestAttemptId = currentAttempt.Id,
                        QuestionId = question.Id
                    };
                    _context.UserAnswers.Add(userAnswer);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TakeTest", new { id = currentAttempt.Id });
        }

        // GET: MockTest/TakeTest/{id} - Trang làm bài thi
        public async Task<IActionResult> TakeTest(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                    .ThenInclude(t => t.Questions.OrderBy(q => q.QuestionNumber))
                .Include(ta => ta.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .Include(ta => ta.User)
                .FirstOrDefaultAsync(ta => ta.Id == id);

            if (attempt == null || attempt.UserId != userId)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài thi.";
                return RedirectToAction(nameof(Index));
            }

            if (attempt.Status != AttemptStatus.InProgress)
            {
                TempData["ErrorMessage"] = "Bài thi này đã hoàn thành hoặc hết hạn.";
                return RedirectToAction("TestResult", new { id = attempt.Id });
            }

            // Tính thời gian còn lại
            var elapsed = (DateTime.Now - attempt.StartTime).TotalSeconds;
            var remaining = (attempt.Test.Duration * 60) - (int)elapsed;

            if (remaining <= 0)
            {
                // Hết giờ - auto submit
                await AutoSubmitAttempt(id);
                return RedirectToAction("TestResult", new { id = id });
            }

            // Group questions by SharedContentGroupId
            var groupedQuestions = attempt.Test.Questions
                .GroupBy(q => (int?)(q.SharedContentGroupId ?? -1))
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new TakeTestViewModel
            {
                TestAttempt = attempt,
                Test = attempt.Test,
                Questions = attempt.Test.Questions.ToList(),
                UserAnswers = attempt.UserAnswers.ToList(),
                TimeRemainingSeconds = remaining,
                StudentName = attempt.User.FullName,
                StudentId = attempt.User.UserName ?? "N/A",
                GroupedQuestions = groupedQuestions
            };

            return View(viewModel);
        }

        // POST: MockTest/SaveAnswer - AJAX save answer
        [HttpPost]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var attempt = await _context.TestAttempts
                    .Include(ta => ta.Test)
                    .Include(ta => ta.UserAnswers)
                    .FirstOrDefaultAsync(ta => ta.Id == request.TestAttemptId && ta.UserId == userId);

                if (attempt == null || attempt.Status != AttemptStatus.InProgress)
                {
                    return Json(new SaveAnswerResponse
                    {
                        Success = false,
                        Message = "Bài thi không hợp lệ hoặc đã kết thúc."
                    });
                }

                // Check time remaining
                var elapsed = (DateTime.Now - attempt.StartTime).TotalSeconds;
                var remaining = (attempt.Test.Duration * 60) - (int)elapsed;

                if (remaining <= 0)
                {
                    await AutoSubmitAttempt(request.TestAttemptId);
                    return Json(new SaveAnswerResponse
                    {
                        Success = false,
                        Message = "Hết thời gian làm bài. Bài thi đã được tự động nộp."
                    });
                }

                var userAnswer = await _context.UserAnswers
                    .FirstOrDefaultAsync(ua => ua.TestAttemptId == request.TestAttemptId && ua.QuestionId == request.QuestionId);

                if (userAnswer != null)
                {
                    var wasNull = userAnswer.SelectedAnswer == null;

                    if (request.SelectedAnswer.HasValue)
                    {
                        userAnswer.SelectedAnswer = request.SelectedAnswer.Value;
                        userAnswer.AnsweredAt = userAnswer.AnsweredAt ?? DateTime.Now;
                        userAnswer.LastModifiedAt = DateTime.Now;

                        if (wasNull)
                        {
                            attempt.TotalAnswered++;
                        }
                    }

                    if (request.IsBookmarked.HasValue)
                    {
                        userAnswer.IsBookmarked = request.IsBookmarked.Value;
                    }

                    attempt.LastSavedAt = DateTime.Now;
                    attempt.TimeRemaining = remaining;

                    await _context.SaveChangesAsync();

                    return Json(new SaveAnswerResponse
                    {
                        Success = true,
                        Message = "Đã lưu câu trả lời.",
                        TotalAnswered = attempt.TotalAnswered,
                        TimeRemaining = remaining
                    });
                }

                return Json(new SaveAnswerResponse
                {
                    Success = false,
                    Message = "Không tìm thấy câu hỏi."
                });
            }
            catch (Exception ex)
            {
                return Json(new SaveAnswerResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // POST: MockTest/SubmitTest - Nộp bài thi
        [HttpPost]
        public async Task<IActionResult> SubmitTest(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                    .ThenInclude(t => t.Questions)
                .Include(ta => ta.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(ta => ta.Id == id && ta.UserId == userId);

            if (attempt == null || attempt.Status != AttemptStatus.InProgress)
            {
                TempData["ErrorMessage"] = "Không thể nộp bài thi này.";
                return RedirectToAction(nameof(Index));
            }

            await SubmitTestAttempt(attempt);

            return RedirectToAction("TestResult", new { id = attempt.Id });
        }

        // GET: MockTest/TestResult/{id} - Hiển thị kết quả
        public async Task<IActionResult> TestResult(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                .FirstOrDefaultAsync(ta => ta.Id == id && ta.UserId == userId);

            if (attempt == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài thi.";
                return RedirectToAction(nameof(Index));
            }

            if (attempt.Status == AttemptStatus.InProgress)
            {
                TempData["ErrorMessage"] = "Bài thi chưa hoàn thành.";
                return RedirectToAction("TakeTest", new { id = id });
            }

            var viewModel = new TestResultViewModel
            {
                TestAttemptId = attempt.Id,
                TestTitle = attempt.Test.Title,
                TotalQuestions = attempt.Test.TotalQuestions,
                TotalAnswered = attempt.TotalAnswered,
                Score = attempt.Score ?? 0,
                TotalPoints = attempt.Test.TotalPoints,
                IsPassed = attempt.IsPassed ?? false,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime ?? DateTime.Now,
                Duration = (attempt.EndTime ?? DateTime.Now) - attempt.StartTime
            };

            return View(viewModel);
        }

        // GET: MockTest/History - Lịch sử thi
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempts = await _context.TestAttempts
                .Include(ta => ta.Test)
                .Where(ta => ta.UserId == userId)
                .OrderByDescending(ta => ta.StartTime)
                .Select(ta => new TestAttemptHistoryItem
                {
                    AttemptId = ta.Id,
                    TestTitle = ta.Test.Title,
                    SubjectCode = ta.Test.SubjectCode,
                    StartTime = ta.StartTime,
                    EndTime = ta.EndTime,
                    Status = ta.Status,
                    Score = ta.Score,
                    IsPassed = ta.IsPassed,
                    AttemptNumber = ta.AttemptNumber,
                    AllowReview = ta.Test.AllowReview
                })
                .ToListAsync();

            var viewModel = new TestHistoryViewModel
            {
                Attempts = attempts
            };

            return View(viewModel);
        }

        // GET: MockTest/GetTimeRemaining - AJAX get remaining time
        [HttpGet]
        public async Task<IActionResult> GetTimeRemaining(long attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                .FirstOrDefaultAsync(ta => ta.Id == attemptId && ta.UserId == userId);

            if (attempt == null)
            {
                return Json(new { success = false });
            }

            var elapsed = (DateTime.Now - attempt.StartTime).TotalSeconds;
            var remaining = (attempt.Test.Duration * 60) - (int)elapsed;

            if (remaining <= 0)
            {
                await AutoSubmitAttempt(attemptId);
                return Json(new { success = true, timeRemaining = 0, autoSubmitted = true });
            }

            return Json(new { success = true, timeRemaining = remaining, autoSubmitted = false });
        }

        #region Helper Methods

        private async Task SubmitTestAttempt(TestAttempt attempt)
        {
            // Chấm điểm
            int correctCount = 0;
            foreach (var userAnswer in attempt.UserAnswers)
            {
                if (userAnswer.SelectedAnswer.HasValue &&
                    userAnswer.SelectedAnswer.Value == userAnswer.Question.CorrectAnswer)
                {
                    userAnswer.IsCorrect = true;
                    correctCount++;
                }
                else
                {
                    userAnswer.IsCorrect = false;
                }
            }

            // Tính điểm
            var pointPerQuestion = attempt.Test.TotalPoints / attempt.Test.TotalQuestions;
            attempt.Score = correctCount * pointPerQuestion;
            attempt.IsPassed = attempt.Score >= attempt.Test.PassScore;
            attempt.EndTime = DateTime.Now;
            attempt.Status = AttemptStatus.Completed;

            await _context.SaveChangesAsync();
        }

        private async Task AutoSubmitAttempt(long attemptId)
        {
            var attempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                    .ThenInclude(t => t.Questions)
                .Include(ta => ta.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(ta => ta.Id == attemptId);

            if (attempt != null && attempt.Status == AttemptStatus.InProgress)
            {
                attempt.Status = AttemptStatus.Expired;
                await SubmitTestAttempt(attempt);
            }
        }

        private static string GetSubjectName(string subjectCode)
        {
            return subjectCode.ToLower() switch
            {
                "math" => "Toán",
                "english" => "Tiếng Anh",
                "physics" => "Vật Lý",
                "chemistry" => "Hóa Học",
                "biology" => "Sinh Học",
                "history" => "Lịch Sử",
                "geography" => "Địa Lý",
                "literature" => "Ngữ Văn",
                "informatics" => "Tin Học",
                "chinese" => "Tiếng Trung Quốc",
                "japanese" => "Tiếng Nhật",
                "korean" => "Tiếng Hàn",
                "russian" => "Tiếng Nga",
                _ => subjectCode
            };
        }

        #endregion
    }
}