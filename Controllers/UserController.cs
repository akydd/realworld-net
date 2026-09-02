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

    [HttpGet(Name = "GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUser([FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Token "))
        {
            return Unauthorized(new { Error = "Authorization header is missing or invalid." });
        }

        var token = authorizationHeader.Substring("Token ".Length).Trim();
        var user = await _userService.GetCurrentUserAsync(token);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, user.Token, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}
