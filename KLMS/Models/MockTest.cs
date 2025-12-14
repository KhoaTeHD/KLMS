using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    #region Enums

    public enum ContentType
    {
        None = 0,
        Text = 1,
        Image = 2,
        PDF = 3,
        Video = 4
    }

    public enum AttemptStatus
    {
        InProgress = 0,
        Completed = 1,
        Expired = 2,
        Abandoned = 3
    }

    #endregion

    #region Core Models

    /// <summary>
    /// Bảng Đề Thi
    /// </summary>
    public class Test
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string SubjectCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int Duration { get; set; } // Thời gian làm bài (phút)

        [Required]
        public int TotalQuestions { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalPoints { get; set; } // VD: 10.00

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PassScore { get; set; } // VD: 5.00

        public bool AllowReview { get; set; } = false;

        public bool AllowMultipleAttempts { get; set; } = true;

        public int? MaxAttempts { get; set; } // null = không giới hạn

        [Required]
        public string CreatedBy { get; set; } // UserId

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime LastModified { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }
    }

    /// <summary>
    /// Bảng Câu Hỏi
    /// </summary>
    public class Question
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long TestId { get; set; }

        [Required]
        public int QuestionNumber { get; set; } // 1, 2, 3...40

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; }

        [Required]
        public ContentType ContentType { get; set; } = ContentType.None;

        [StringLength(500)]
        public string? ContentData { get; set; } // Text content hoặc file URL

        public int? SharedContentGroupId { get; set; } // Nhóm câu dùng chung content

        [Required]
        [StringLength(500)]
        public string AnswerA { get; set; }

        [Required]
        [StringLength(500)]
        public string AnswerB { get; set; }

        [Required]
        [StringLength(500)]
        public string AnswerC { get; set; }

        [Required]
        [StringLength(500)]
        public string AnswerD { get; set; }

        [Required]
        public char CorrectAnswer { get; set; } // 'A', 'B', 'C', 'D'

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Point { get; set; }

        [StringLength(1000)]
        public string? Explanation { get; set; }

        // Navigation properties
        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    /// <summary>
    /// Bảng Lượt Thi
    /// </summary>
    public class TestAttempt
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long TestId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public int TimeRemaining { get; set; } // Giây còn lại

        [Required]
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        public int TotalAnswered { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        public bool? IsPassed { get; set; }

        [Required]
        public int AttemptNumber { get; set; } = 1;

        [Required]
        public DateTime LastSavedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    /// <summary>
    /// Bảng Câu Trả Lời
    /// </summary>
    public class UserAnswer
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long TestAttemptId { get; set; }

        [Required]
        public long QuestionId { get; set; }

        public char? SelectedAnswer { get; set; } // 'A', 'B', 'C', 'D' hoặc null

        public bool IsBookmarked { get; set; } = false;

        public bool? IsCorrect { get; set; }

        public DateTime? AnsweredAt { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        // Navigation properties
        [ForeignKey("TestAttemptId")]
        public virtual TestAttempt TestAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }

    #endregion

    #region ViewModels (for legacy compatibility - keep for Index page)

    public class StudentTestInfo
    {
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int Session { get; set; }
    }

    public class TestCouncil
    {
        public string CouncilName { get; set; }
        public string TestLocation { get; set; }
        public string Room { get; set; }
    }

    public class TestSubject
    {
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public bool IsActive { get; set; }
        public string Url { get; set; }
        public long? TestId { get; set; } // Link to actual Test
    }

    public class MockTestViewModel
    {
        public StudentTestInfo StudentInfo { get; set; }
        public TestCouncil Council { get; set; }
        public List<TestSubject> Subjects { get; set; }

        public MockTestViewModel()
        {
            StudentInfo = new StudentTestInfo();
            Council = new TestCouncil();
            Subjects = new List<TestSubject>();
        }
    }

    #endregion
}