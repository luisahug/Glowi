using Microsoft.AspNetCore.Identity;

namespace Glowi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
    }
}
