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

    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        int? userId = int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
        var profile = await _profileService.GetProfileByUsernameAsync(username, userId);
        if (profile == null)
        {
            return NotFound();
        }
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }

    [Authorize]
    [HttpPost("{username}/follow")]
    public async Task<IActionResult> FollowUser(string username)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var profile = await _profileService.FollowUserAsync(username, userId);
        if (profile == null)
        {
            return NotFound();
        }
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }

    [Authorize]
    [HttpDelete("{username}/follow")]
    public async Task<IActionResult> UnfollowUser(string username)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var profile = await _profileService.UnfollowUserAsync(username, userId);
        if (profile == null)
        {
            return NotFound();
        }
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }
}
