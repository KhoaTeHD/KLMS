using KLMS.Data;
using KLMS.Models;
using Microsoft.AspNetCore.Identity;

namespace KLMS.Data
{
    /// <summary>
    /// Seed data cho Mock Test
    /// Tạo 2 đề thi mẫu: Tiếng Anh (với shared content) và Toán (individual content)
    /// </summary>
    public static class MockTestSeeder
    {
        public static async Task SeedMockTestData(ApplicationDbContext context, UserManager<User> userManager)
        {
            // Check if already seeded
            if (context.Tests.Any())
            {
                return; // Data already exists
            }

            // Get an admin/teacher user to be the creator
            var admin = await userManager.FindByEmailAsync("admin@klms.com");
            if (admin == null)
            {
                // Create a default admin for testing
                admin = new User
                {
                    UserName = "admin@klms.com",
                    Email = "admin@klms.com",
                    FullName = "Quản trị viên",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ============================================
            // TEST 1: TIẾNG ANH (Với Shared Content)
            // ============================================
            var englishTest = new Test
            {
                Title = "Đề thi thử THPT 2025 - Tiếng Anh",
                SubjectCode = "english",
                Description = "Đề thi thử môn Tiếng Anh THPT Quốc gia 2025",
                Duration = 60, // 60 phút
                TotalQuestions = 10,
                TotalPoints = 10.0m,
                PassScore = 5.0m,
                AllowReview = true,
                AllowMultipleAttempts = true,
                MaxAttempts = 3,
                CreatedBy = admin.Id,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                IsActive = true
            };

            context.Tests.Add(englishTest);
            await context.SaveChangesAsync();

            // Reading Comprehension - Shared Content Group 1 (Questions 1-4)
            string readingPassage = @"The concept of project farming, where farmers come together to collaborate on large-scale agricultural projects, has gained significant traction, and modern technology keeps this collaboration on track. Advanced technologies such as GPS, sensors, drones, and data analytics are used to optimise agricultural practices. Additionally, collected real-time data on soil conditions, weather patterns, and plant growth enables farmers to accelerate the decision-making process that maximises productivity while minimising resource wastage.

GPS technology allows farmers to precisely map out their fields and create customised planting plans. This ensures that seeds are sown at optimal locations based on soil characteristics and previous yield data. By avoiding areas with poor fertility, farmers can increase their overall crop yield. Camera traps provide advance warnings of insects, so farmers do not have to treat the whole field. This, therefore, helps curb chemical runoff and save money for every party involved in this project.

Technology also plays a vital role in optimising irrigation practices for sustainable agriculture. Specialised equipment reports dryness hour by hour, and weather apps forecast rain for the week ahead. Automated valves give each zone exactly the water it needs and pause when clouds are approaching. This cuts pumping costs and protects groundwater while keeping the crop healthy. On dry continents, such savings keep projects economically viable.

The digital trail does not stop at the farm gate. Cloud platforms let project farmers, processors, and truck drivers input harvest weights, storage temperatures, and delivery times the moment they change, while blockchain records freeze each entry so customers can trust it. Analytic tools combine seasons of records to forecast demand, spot price opportunities, and mark weak points in the workflow. This allows project farmers to anticipate market demand, exploit resource allocation, and plan for potential challenges.";

            var englishQuestions = new List<Question>
            {
                // Group 1: Reading Comprehension (Questions 1-4)
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 1,
                    QuestionText = "Which of the following is NOT mentioned in paragraph 1 as a type of collected real-time data?",
                    ContentType = ContentType.Text,
                    ContentData = readingPassage,
                    SharedContentGroupId = 1,
                    AnswerA = "data analytics",
                    AnswerB = "weather patterns",
                    AnswerC = "plant growth",
                    AnswerD = "soil conditions",
                    CorrectAnswer = 'A',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 2,
                    QuestionText = "The word 'accelerate' in paragraph 1 can be best replaced by ______.",
                    ContentType = ContentType.Text,
                    ContentData = readingPassage,
                    SharedContentGroupId = 1,
                    AnswerA = "speed",
                    AnswerB = "require",
                    AnswerC = "install",
                    AnswerD = "guide",
                    CorrectAnswer = 'A',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 3,
                    QuestionText = "The word 'curb' in paragraph 2 is OPPOSITE in meaning to ______.",
                    ContentType = ContentType.Text,
                    ContentData = readingPassage,
                    SharedContentGroupId = 1,
                    AnswerA = "increase",
                    AnswerB = "limit",
                    AnswerC = "monitor",
                    AnswerD = "reduce",
                    CorrectAnswer = 'A',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 4,
                    QuestionText = "The word 'it' in paragraph 3 refers to ______.",
                    ContentType = ContentType.Text,
                    ContentData = readingPassage,
                    SharedContentGroupId = 1,
                    AnswerA = "zone",
                    AnswerB = "week",
                    AnswerC = "equipment",
                    AnswerD = "water",
                    CorrectAnswer = 'D',
                    Point = 1.0m
                },
                
                // Individual Questions (5-10)
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 5,
                    QuestionText = "Choose the word whose underlined part differs from the other three: laughed, watched, stopped, raised",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "laughed",
                    AnswerB = "watched",
                    AnswerC = "stopped",
                    AnswerD = "raised",
                    CorrectAnswer = 'D',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 6,
                    QuestionText = "She ______ to the library every Saturday to borrow books.",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "go",
                    AnswerB = "goes",
                    AnswerC = "going",
                    AnswerD = "gone",
                    CorrectAnswer = 'B',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 7,
                    QuestionText = "If I ______ enough money, I would buy a new car.",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "have",
                    AnswerB = "had",
                    AnswerC = "have had",
                    AnswerD = "will have",
                    CorrectAnswer = 'B',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 8,
                    QuestionText = "The house ______ by the storm last night.",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "destroyed",
                    AnswerB = "was destroyed",
                    AnswerC = "is destroyed",
                    AnswerD = "destroys",
                    CorrectAnswer = 'B',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 9,
                    QuestionText = "Choose the best answer: 'How long have you been learning English?' - '______'",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "Since 5 years",
                    AnswerB = "For 5 years",
                    AnswerC = "5 years ago",
                    AnswerD = "In 5 years",
                    CorrectAnswer = 'B',
                    Point = 1.0m
                },
                new Question
                {
                    TestId = englishTest.Id,
                    QuestionNumber = 10,
                    QuestionText = "She is ______ beautiful singer that everyone loves her voice.",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "so",
                    AnswerB = "such",
                    AnswerC = "such a",
                    AnswerD = "so much",
                    CorrectAnswer = 'C',
                    Point = 1.0m
                }
            };

            context.Questions.AddRange(englishQuestions);

            // ============================================
            // TEST 2: TOÁN (Individual Content)
            // ============================================
            var mathTest = new Test
            {
                Title = "Đề thi thử THPT 2025 - Toán",
                SubjectCode = "math",
                Description = "Đề thi thử môn Toán THPT Quốc gia 2025",
                Duration = 90, // 90 phút
                TotalQuestions = 8,
                TotalPoints = 10.0m,
                PassScore = 5.0m,
                AllowReview = true,
                AllowMultipleAttempts = true,
                MaxAttempts = 3,
                CreatedBy = admin.Id,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                IsActive = true
            };

            context.Tests.Add(mathTest);
            await context.SaveChangesAsync();

            var mathQuestions = new List<Question>
            {
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 1,
                    QuestionText = "Tính giá trị của biểu thức: 2 + 3 × 4",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "20",
                    AnswerB = "14",
                    AnswerC = "24",
                    AnswerD = "12",
                    CorrectAnswer = 'B',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 2,
                    QuestionText = "Giải phương trình: 2x + 5 = 13",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "x = 4",
                    AnswerB = "x = 3",
                    AnswerC = "x = 5",
                    AnswerD = "x = 6",
                    CorrectAnswer = 'A',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 3,
                    QuestionText = "Tính đạo hàm của hàm số f(x) = x²",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "f'(x) = x",
                    AnswerB = "f'(x) = 2x",
                    AnswerC = "f'(x) = x²",
                    AnswerD = "f'(x) = 2",
                    CorrectAnswer = 'B',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 4,
                    QuestionText = "Tích phân ∫₀¹ x dx bằng:",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "1/2",
                    AnswerB = "1",
                    AnswerC = "2",
                    AnswerD = "1/3",
                    CorrectAnswer = 'A',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 5,
                    QuestionText = "Số nghiệm của phương trình x² - 4 = 0 là:",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "0",
                    AnswerB = "1",
                    AnswerC = "2",
                    AnswerD = "3",
                    CorrectAnswer = 'C',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 6,
                    QuestionText = "log₂(8) = ?",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "2",
                    AnswerB = "3",
                    AnswerC = "4",
                    AnswerD = "8",
                    CorrectAnswer = 'B',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 7,
                    QuestionText = "Giá trị của sin(30°) là:",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "1/2",
                    AnswerB = "√3/2",
                    AnswerC = "1",
                    AnswerD = "√2/2",
                    CorrectAnswer = 'A',
                    Point = 1.25m
                },
                new Question
                {
                    TestId = mathTest.Id,
                    QuestionNumber = 8,
                    QuestionText = "Hệ số góc của đường thẳng y = 3x + 2 là:",
                    ContentType = ContentType.None,
                    SharedContentGroupId = null,
                    AnswerA = "2",
                    AnswerB = "3",
                    AnswerC = "5",
                    AnswerD = "1",
                    CorrectAnswer = 'B',
                    Point = 1.25m
                }
            };

            context.Questions.AddRange(mathQuestions);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Mock Test seed data created successfully!");
            Console.WriteLine($"   - English Test: {englishTest.TotalQuestions} questions");
            Console.WriteLine($"   - Math Test: {mathTest.TotalQuestions} questions");
        }
    }
}

/*
 * HƯỚNG DẪN SỬ DỤNG:
 * 
 * 1. Thêm code gọi seeder vào Program.cs (hoặc Startup.cs):
 * 
 * using (var scope = app.Services.CreateScope())
 * {
 *     var services = scope.ServiceProvider;
 *     var context = services.GetRequiredService<ApplicationDbContext>();
 *     var userManager = services.GetRequiredService<UserManager<User>>();
 *     
 *     await MockTestSeeder.SeedMockTestData(context, userManager);
 * }
 * 
 * 2. Chạy ứng dụng, seed data sẽ tự động được tạo nếu chưa có
 * 
 * 3. Kiểm tra database:
 *    - Bảng Tests: 2 records (English, Math)
 *    - Bảng Questions: 18 records (10 English + 8 Math)
 */
