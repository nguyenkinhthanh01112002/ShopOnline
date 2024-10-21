using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopWebAPI.DTOs.User;
using ShopWebAPI.Interfaces.User;
using ShopWebAPI.Models;

namespace ShopWebAPI.Controllers
{
  
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UserController(IUserService userService,IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            var userDto = _mapper.Map<List<UserDto>>(users);
            return Ok(userDto);
        }
       
        [HttpPut]
        [Route("{username}")]
        public async Task<IActionResult> EditUser(string username, [FromBody]RequestUserDto? userDto)
        {
            try
            {
                var result = await _userService.EditUser(username, userDto);
                if (result.Success) { return Ok(result); }
                return BadRequest(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        [HttpDelete]
        [Route("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var result = await _userService.DeleteUser(username);
                if (result.Success) { return Ok(result); }
                return BadRequest(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
