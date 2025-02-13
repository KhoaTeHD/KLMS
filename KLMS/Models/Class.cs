using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    public class Class
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string ClassName { get; set; }

        private string Description { get; set; }

        [DataType(DataType.Date)]
        private DateOnly CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        private DateTime LastModified { get; set; }

        private string? TeacherId;

        [ForeignKey("TeacherId")]
        public User? Teacher { get; set; }

        public ICollection<Lecture>? Lectures { get; set; }

        public ICollection<User>? Students { get; set; }
    }
}
