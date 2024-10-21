using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.DTOs.Auth.ForgotPassWord
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
    }
}
