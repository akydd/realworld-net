using Microsoft.AspNetCore.Mvc;
using realworld_net.Dtos;
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
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto userDto)
    {
        var user = await _userService.RegisterUserAsync(userDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }

    [HttpPost("login", Name = "LoginUser")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserDto userDto)
    {
        var user = await _userService.LoginUserAsync(userDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}
