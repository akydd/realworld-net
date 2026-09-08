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

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="userDto">New user information.</param>
    /// <returns>The details of the newly created user.</returns>
    /// <response code="201">When a new user is created.</response>
    /// <response code="409">When the email and/or username has already been taken.</response>
    /// <response code="422">When the request body is invalid.</response>
    [HttpPost(Name = "RegisterUser")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto userDto)
    {
        var user = await _userService.RegisterUserAsync(userDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token!, user.Username, user.Bio, user.Image));
        return Created("", userResponse);
    }

    /// <summary>
    /// User login.
    /// </summary>
    /// <param name="userDto">Login credentials.</param>
    /// <returns>The details of the logged in user, including the authentication token.</returns>
    /// <response code="200">For a successful login.</response>
    /// <response code="422">When the request body is invalid.</response>
    /// <response code="401">When the login credentials are invalid.</response>
    [HttpPost("login", Name = "LoginUser")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserDto userDto)
    {
        var user = await _userService.LoginUserAsync(userDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}
