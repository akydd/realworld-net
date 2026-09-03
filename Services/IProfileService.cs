using realworld_net.Models;

namespace realworld_net.Services;

public interface IProfileService
{
    Task<Profile> GetProfileByUsernameAsync(string username, int? currentUserId);
    Task<Profile> FollowUserAsync(string username, int currentUserId);
    Task<Profile> UnfollowUserAsync(string username, int currentUserId);
}
