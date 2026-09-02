using Microsoft.AspNetCore.Mvc;
using realworld_net.Dtos;
using realworld_net.Models;
using realworld_net.Services;

namespace realworld_net.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{

    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost(Name = "RegisterUser")]
    public IActionResult RegisterUser([FromBody] RegisterUserDto userDto)
    {
        // Handle the user data here
        var user = _userService.RegisterUserAsync(userDto).Result;
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}