using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.DTOs.User
{
    public class RequestUserDto
    {
     
        public string? FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
