using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.DTOs.Auth.Login
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public bool? IsRemember {  get; set; }   
    }
}
