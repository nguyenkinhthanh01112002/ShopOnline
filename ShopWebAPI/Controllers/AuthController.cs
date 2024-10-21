using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWebAPI.Data;
using ShopWebAPI.DTOs.Auth.ForgotPassWord;
using ShopWebAPI.DTOs.Auth.Login;
using ShopWebAPI.DTOs.Auth.Register;
using ShopWebAPI.DTOs.Auth.Token;
using ShopWebAPI.DTOs.Response;
using ShopWebAPI.Interfaces.Auth;
using ShopWebAPI.Models;
using ShopWebAPI.Services.Auth;
using ShopWebAPI.Utilities;
namespace ShopWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
   
        public AuthController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager,ITokenService tokenService,IEmailService emailService,ApplicationDbContext context)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._tokenService = tokenService;
            this._emailService = emailService;
            this._context = context;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user)
        {
            try
            {
                if(!ModelState.IsValid) {return BadRequest(ModelState); }
                var appUser = new ApplicationUser
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    UserName = user.Email
                };
                //create a user here
                var result = await _userManager.CreateAsync(appUser,user.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                //addrole here
                if(! await _roleManager.RoleExistsAsync(UserRole.User))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.User));
                }
               if(await _roleManager.RoleExistsAsync(UserRole.User))
               {
                    await _userManager.AddToRoleAsync(appUser,UserRole.User);
               }
                return Created();
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
                
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid request body.");
                }
                var user = await _userManager.FindByNameAsync(loginRequestDto.UserName);
                if (user == null)
                {
                    return Unauthorized("Invalid credentials");
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, true);
                if (result.IsLockedOut)
                {
                    return Unauthorized("Account is locked out. Please try again later 1 minute.");
                }
                else if(!result.Succeeded)
                {
                    return Unauthorized("Invalid username or password");
                }
                else
                {          
                    string AccessToken = await _tokenService.IssueAccessToken(user);
                    string RefreshToken = _tokenService.IssueRefreshToken();
                    bool check = await _tokenService.SaveRefreshToken(loginRequestDto.UserName, RefreshToken);
                    if (check) { return Ok(new LoginResponseDto { AccessToken = AccessToken, RefreshToken = RefreshToken }); }
                    else
                    {
                        // Không thể lưu refresh token
                        return StatusCode(500, new
                        {
                            Error = "Login successful but unable to create session",
                            Details = "Failed to save refresh token",
                            AccessToken = AccessToken,
                            RefreshToken = RefreshToken
                        });
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto token)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Refresh token is required!!!");
                }
                var userName = await _tokenService.RetrieveUsernameByRefreshToken(token.Token);
                if (string.IsNullOrEmpty(userName)) { return Unauthorized("Invalid refresh token"); }
                var appUser = await _userManager.FindByNameAsync(userName);
                if (appUser == null) { return Unauthorized("Invalid User"); }
                string accessToken = await _tokenService.IssueAccessToken(appUser);
                string refreshToken =  _tokenService.IssueRefreshToken();
                await _tokenService.SaveRefreshToken(userName, refreshToken);
                return Ok (new {AccessToken = accessToken, RefreshToken = refreshToken});
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        [HttpPost("ForgotPassword")]

        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
                return BadRequest("Invalid Email");// Don't reveal that the user does not exist

            var verificationCode = VerificationCodeGenerator.GenerateCode();
            user.RefreshToken = verificationCode;
            user.ExpiredTime = DateTime.UtcNow.AddMinutes(2); // Token expires after 2 minute

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, "An error occurred while processing your request.");

            await _emailService.SendEmailAsync(user.Email, "ResetPassword Verifition",
                $"Your verification code is: {verificationCode}. This code will expire in 2 minute.");

            return Ok(new { verificationCode });
        }

        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyPasswordRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.RefreshToken != dto.Code ||
                user.ExpiredTime < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired code.");
            }
            string accessToken = await _tokenService.IssueAccessToken(user);         
            return Ok(new { accessToken = accessToken, refreshToken = dto.Code });
        }

        [Authorize(Roles = "user")]
        [HttpPost("ResetPassword")]      
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Information");
                }
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == resetPasswordDto.RefreshToken);
                if (user == null) return BadRequest("Refresh Token invalid");
                string tokenResetPassword = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user,tokenResetPassword, resetPasswordDto.Password);
                if (result.Succeeded) {
                   return Ok(new ResponseRequest
                    {
                        Message = "Password reseted successfully",
                        Success = true
                    });
                }
                return BadRequest(result.Errors);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }


        }
    }
}
