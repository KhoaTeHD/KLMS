using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    public class Lecture
    {
        public long Id { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModified { get; set; }

        public long ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }
    }
}
