using System.ComponentModel.DataAnnotations;

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

        private int TeacherId;
    }
}
