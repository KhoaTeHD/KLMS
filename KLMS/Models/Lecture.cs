namespace KLMS.Models
{
    public class Lecture
    {
        public long Id { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModified { get; set; }
    }
}
