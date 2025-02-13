using Microsoft.AspNetCore.Identity;

namespace KLMS.Models
{
    public class User : IdentityUser
    {
        public ICollection<Class>? Classes { get; set; }
    }
}
