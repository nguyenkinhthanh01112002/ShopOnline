using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ShopWebAPI.DTOs.Auth.Register
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }
        [DataType(DataType.Date)]      
        
        public DateTime DateOfBirth { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
