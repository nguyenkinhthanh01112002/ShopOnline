using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ShopWebAPI.DTOs.Auth.ForgotPassWord
{
    public class ResetPasswordDto
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string RefreshToken { get; set; }    
    }
}
