using Microsoft.AspNetCore.Identity;
using POS.Api.Enums;

namespace POS.Api.Models
{
    public class AppUser : IdentityUser
    {
        public int BranchId { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        public virtual Branch Branch { get; set; } = null!;
    }
}
