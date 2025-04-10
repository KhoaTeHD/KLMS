using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    public class Class
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ClassName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModified { get; set; }

        public string? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        [InverseProperty("ClassesTaught")]
        public User? Teacher { get; set; }

        public ICollection<Lecture>? Lectures { get; set; }

        public ICollection<User> Students { get; set; } = new List<User>();
    }
}
