using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopWebAPI.Data;
using ShopWebAPI.DTOs.Response;
using ShopWebAPI.DTOs.User;
using ShopWebAPI.Interfaces.User;
using ShopWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ShopWebAPI.Services.User
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<ResponseRequest> EditUser(string userName, RequestUserDto? requestUserDto)
        {
            // neu khong xac thuc no se khong cho tiep can phuong thuc vi co che middleware
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return new ResponseRequest
                {
                    Success = false,
                    Message = "Information updated fail"
                };
            }
            
            user.FullName = !string.IsNullOrEmpty(requestUserDto.FullName) ? requestUserDto.FullName : user.FullName;
            user.Email = !string.IsNullOrEmpty(requestUserDto.Email) ? requestUserDto.Email : user.Email;
            user.DateOfBirth = (DateTime) (!string.IsNullOrEmpty(requestUserDto.DateOfBirth.ToString()) ? requestUserDto.DateOfBirth : user.DateOfBirth);
            user.UserName = user.Email;
            await _context.SaveChangesAsync();
            return new ResponseRequest
            {
                Success = true,
                Message = "Information updated successfully"
            };
        }

        public async Task<List<ApplicationUser>?> GetUsers()
        {
           return await _context.Users.ToListAsync();
        }
        public async Task<ResponseRequest> DeleteUser (string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if(user == null)
            {
                return new ResponseRequest 
                {
                    Message = "Deleted user fail",
                    Success = false
                };
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new ResponseRequest
            {
                Message = "Deleted user successful",
                Success = true
            };
        }
    }
}
