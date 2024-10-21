namespace ShopWebAPI.DTOs.Auth.Login
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
