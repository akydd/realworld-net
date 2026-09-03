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
        var profile = await _profileService.GetProfileByUsernameAsync(username, null);
        if (profile == null)
        {
            return NotFound();
        }
        var profileResponse = new ProfileResponseDto(new ProfileResponseInnerDto(profile.Username, profile.Bio, profile.Image, profile.Following));
        return Ok(profileResponse);
    }
}
