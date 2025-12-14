using KLMS.Models;

namespace KLMS.ViewModels
{
    /// <summary>
    /// ViewModel cho trang làm bài thi
    /// </summary>
    public class TakeTestViewModel
    {
        public TestAttempt TestAttempt { get; set; }
        public Test Test { get; set; }
        public List<Question> Questions { get; set; }
        public List<UserAnswer> UserAnswers { get; set; }
        public int TimeRemainingSeconds { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }

        // Grouped questions by SharedContentGroupId
        public Dictionary<int?, List<Question>> GroupedQuestions { get; set; }

        public TakeTestViewModel()
        {
            Questions = new List<Question>();
            UserAnswers = new List<UserAnswer>();
            GroupedQuestions = new Dictionary<int?, List<Question>>();
        }
    }

    /// <summary>
    /// ViewModel cho kết quả thi
    /// </summary>
    public class TestResultViewModel
    {
        public long TestAttemptId { get; set; }
        public string TestTitle { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAnswered { get; set; }
        public decimal Score { get; set; }
        public decimal TotalPoints { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// ViewModel cho lịch sử thi
    /// </summary>
    public class TestHistoryViewModel
    {
        public List<TestAttemptHistoryItem> Attempts { get; set; }

        public TestHistoryViewModel()
        {
            Attempts = new List<TestAttemptHistoryItem>();
        }
    }

    public class TestAttemptHistoryItem
    {
        public long AttemptId { get; set; }
        public string TestTitle { get; set; }
        public string SubjectCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public AttemptStatus Status { get; set; }
        public decimal? Score { get; set; }
        public bool? IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        public bool AllowReview { get; set; }
    }

    /// <summary>
    /// DTO cho AJAX save answer
    /// </summary>
    public class SaveAnswerRequest
    {
        public long TestAttemptId { get; set; }
        public long QuestionId { get; set; }
        public char? SelectedAnswer { get; set; }
        public bool? IsBookmarked { get; set; }
    }

    /// <summary>
    /// DTO cho AJAX response
    /// </summary>
    public class SaveAnswerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalAnswered { get; set; }
        public int TimeRemaining { get; set; }
    }

    /// <summary>
    /// ViewModel cho export Excel
    /// </summary>
    public class ExportTestResultViewModel
    {
        public string TestTitle { get; set; }
        public string SubjectCode { get; set; }
        public DateTime ExportDate { get; set; }
        public List<StudentResultRow> Students { get; set; }
        public List<QuestionAnalysisRow> QuestionAnalysis { get; set; }

        public ExportTestResultViewModel()
        {
            Students = new List<StudentResultRow>();
            QuestionAnalysis = new List<QuestionAnalysisRow>();
        }
    }

    public class StudentResultRow
    {
        public int STT { get; set; }
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public decimal Score { get; set; }
        public string Result { get; set; } // "Đạt" / "Không đạt"
        public Dictionary<int, bool> QuestionResults { get; set; } // Question# -> true/false
    }

    public class QuestionAnalysisRow
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public char CorrectAnswer { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalStudents { get; set; }
        public decimal CorrectPercentage { get; set; }
    }
}
