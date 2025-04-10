using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    public class User : IdentityUser
    {
        [StringLength(200)]
        public string FullName { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Class> Classes { get; set; } = new List<Class>();

        [InverseProperty("Teacher")]
        public ICollection<Class> ClassesTaught { get; set; } = new List<Class>();
    }
}
