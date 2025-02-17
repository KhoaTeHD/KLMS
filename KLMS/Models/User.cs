using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLMS.Models
{
    public class User : IdentityUser
    {
        public ICollection<Class> Classes { get; set; } = new List<Class>();

        [InverseProperty("Teacher")]
        public ICollection<Class> ClassesTaught { get; set; } = new List<Class>();
    }
}
