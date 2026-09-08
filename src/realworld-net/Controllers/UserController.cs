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

    /// <summary>
    /// Return the details of the authenticated user.
    /// </summary>
    /// <returns>The user record of the user matching the authentication token.</returns>
    /// <response code="200">The user is authenticated</response>
    /// <response code="401">The caller is not authenticated.</response>
    [Authorize]
    [HttpGet(Name = "GetCurrentUser")]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var token = await HttpContext.GetTokenAsync("access_token");
        var user = await _userService.GetUserByIdAsync(userId);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="updateUserDto">The user fields to update.</param>
    /// <returns>The updated user details.</returns>
    /// <response code="200">The user is successfully updated.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="409">The updated username or email are already taken.</response>
    [Authorize]
    [HttpPut(Name = "UpdateUser")]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var token = await HttpContext.GetTokenAsync("access_token");
        var user = await _userService.UpdateUserAsync(userId, updateUserDto);
        var userResponse = new UserResponseDto(new UserResponseInnerDto(user.Email, token!, user.Username, user.Bio, user.Image));
        return Ok(userResponse);
    }
}
