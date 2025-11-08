using System.ComponentModel.DataAnnotations;

namespace KLMS.Models
{
    // Model cho thông tin thí sinh
    public class StudentTestInfo
    {
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int Session { get; set; }
    }

    // Model cho hội đồng thi
    public class TestCouncil
    {
        public string CouncilName { get; set; }
        public string TestLocation { get; set; }
        public string Room { get; set; }
    }

    // Model cho môn thi
    public class TestSubject
    {
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public bool IsActive { get; set; }
        public string Url { get; set; }
    }

    // ViewModel tổng hợp cho trang Mock Test
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
}