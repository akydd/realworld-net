using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realworld_net.Dtos;
using realworld_net.Services;

namespace realworld_net.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet(Name = "GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var token = await HttpContext.GetTokenAsync("access_token");
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { Error = "User not found." });
        }
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }

    [Authorize]
    [HttpPut(Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var token = await HttpContext.GetTokenAsync("access_token");
        var user = await _userService.UpdateUserAsync(userId, updateUserDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}
