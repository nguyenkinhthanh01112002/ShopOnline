using ShopWebAPI.Models;
using System.Security.Claims;

namespace ShopWebAPI.Interfaces.Auth
{
    public interface ITokenService
    {
        public Task<string> IssueAccessToken(ApplicationUser app);
        public string IssueRefreshToken();
        public Task<bool> SaveRefreshToken(string userName, string refreshToken);

        public Task<string?> RetrieveUsernameByRefreshToken(string refreshToken);
    }
}
