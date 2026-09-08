using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realworld_net.Dtos;
using realworld_net.Services;

namespace realworld_net.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Returns a user's profile.
    /// </summary>
    /// <param name="username">The unique username.</param>
    /// <returns>The user's profile.</returns>
    /// <response code="200">The user has a profile.</response>
    /// <response code="404">No such user exists.</response>
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(string username)
    {
        int? userId = int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
        var profile = await _profileService.GetProfileByUsernameAsync(username, userId);
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }

    /// <summary>
    /// Follow a user.
    /// </summary>
    /// <param name="username">The unique username of the user to follow.</param>
    /// <returns>The followed user's profile.</returns>
    /// <response code="200">The authenticated user follows the user.</response>
    /// <response code="404">No such user exists.</response>
    /// <response code="401">The caller has not authenticated.</response>
    [Authorize]
    [HttpPost("{username}/follow")]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> FollowUser(string username)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var profile = await _profileService.FollowUserAsync(username, userId);
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }

    /// <summary>
    /// Unfollow a user.
    /// </summary>
    /// <param name="username">The unique username of the user to unfollow.</param>
    /// <returns>The unfollowed user's profile.</returns>
    /// <response code="200">The authenticated user unfollows the user.</response>
    /// <response code="404">No such user exists.</response>
    /// <response code="401">The caller has not authenticated.</response>
    [Authorize]
    [HttpDelete("{username}/follow")]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UnfollowUser(string username)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var profile = await _profileService.UnfollowUserAsync(username, userId);
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }
}
