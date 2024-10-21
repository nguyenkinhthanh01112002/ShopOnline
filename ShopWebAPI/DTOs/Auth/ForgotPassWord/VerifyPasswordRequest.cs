using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.DTOs.Auth.ForgotPassWord
{
    public class VerifyPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]  
        public string Code { get; set; }
    }
}
