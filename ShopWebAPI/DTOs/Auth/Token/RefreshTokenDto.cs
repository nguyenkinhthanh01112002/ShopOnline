using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.DTOs.Auth.Token
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; }
    }
}
