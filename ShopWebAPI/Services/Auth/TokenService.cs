using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopWebAPI.Data;
using ShopWebAPI.Interfaces.Auth;
using ShopWebAPI.Models;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopWebAPI.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userMangager;
        private readonly SymmetricSecurityKey _key;
        private readonly ApplicationDbContext _context;

        public TokenService(IConfiguration configuration,UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this._configuration = configuration;
            this._userMangager = userManager;
            this._key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            this._context = context;
        }
        public async Task<string> IssueAccessToken(ApplicationUser app)
        {
            // setup the signing credentials to user token
            var credentials = new SigningCredentials(_key,SecurityAlgorithms.HmacSha256);
            // Prepare claims to be included in the token.
            var claims = new List<Claim>
            {
                new Claim("App_Id",app.Id),
                new Claim(ClaimTypes.NameIdentifier,app.UserName),
                new Claim(ClaimTypes.Email,app.Email),
                new Claim(JwtRegisteredClaimNames.Sub,app.Id)
            };
            var roles = await _userMangager.GetRolesAsync(app);
            //add role claims for the user
            foreach (var role in roles)
            { 
                claims.Add(new Claim(ClaimTypes.Role,role.ToString()));
            }

            //add role claims for the user
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(2),
                signingCredentials:credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public string IssueRefreshToken()
        {
            var randomNumber = new byte[32];  // Prepare a buffer to hold the random bytes.
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);  // Fill the buffer with cryptographically strong random bytes.
                return Convert.ToBase64String(randomNumber);  // Convert the bytes to a Base64 string and return.
            }
        }
        // ham nay duoc su dung sau khi login thanh cong nen dam bao userName luon dung
        public async Task<bool> SaveRefreshToken(string userName, string refreshToken)
        {
            var user = await _userMangager.FindByNameAsync(userName);
            if (user == null)
            {
                return false;
            }
            user.RefreshToken = refreshToken;
            user.ExpiredTime = string.IsNullOrEmpty(user.ExpiredTime.ToString()) ? DateTime.UtcNow.AddDays(7) : user.ExpiredTime;
            var result = await _userMangager.UpdateAsync(user);     
            if(result.Succeeded) {return true;}
            return false;
        }

        public async Task<string?> RetrieveUsernameByRefreshToken(string refreshToken)
        {
           var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.ExpiredTime>DateTime.UtcNow);
           return user?.UserName;
        }

      
    }
}
