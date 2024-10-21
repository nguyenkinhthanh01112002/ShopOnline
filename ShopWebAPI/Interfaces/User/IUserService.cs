using ShopWebAPI.DTOs.Response;
using ShopWebAPI.DTOs.User;
using ShopWebAPI.Models;

namespace ShopWebAPI.Interfaces.User
{
    public interface IUserService
    {
        public Task<List<ApplicationUser>?> GetUsers();
        public Task<ResponseRequest> EditUser (string userName,RequestUserDto? requestUserDto);
        public Task<ResponseRequest> DeleteUser (string userName);
    }
}
