using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopWebAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiredTime { get; set; }
    }
}
