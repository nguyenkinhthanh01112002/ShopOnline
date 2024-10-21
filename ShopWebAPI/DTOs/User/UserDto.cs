using ShopWebAPI.Migrations;

namespace ShopWebAPI.DTOs.User
{
    public class UserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public DateTime DateOfBirth { get; set; }

    }
}
