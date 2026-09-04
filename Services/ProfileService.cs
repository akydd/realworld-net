using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Models;

namespace realworld_net.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Profile> GetProfileByUsernameAsync(string username, int? currentUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");
        bool isFollowing = false;

        if (currentUserId.HasValue)
        {
            isFollowing = await _context.Follows.AnyAsync(f => f.FollowerId == currentUserId.Value && f.FolloweeId == user.Id);
        }

        return new Profile
        (
            user.Username,
            user.Bio,
            user.Image,
            isFollowing
        );
    }

    public async Task<Profile> FollowUserAsync(string username, int currentUserId)
    {
        var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");

        _context.Follows.Add(new Entities.Follows
        {
            FollowerId = currentUserId,
            FolloweeId = userToFollow.Id
        });
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            // Do nothing, user is already following.
        }

        return new Profile
        (
            userToFollow.Username,
            userToFollow.Bio,
            userToFollow.Image,
            true
        );
    }

    public async Task<Profile> UnfollowUserAsync(string username, int currentUserId)
    {
        var userToUnfollow = await _context.Users.FirstOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");

        await _context.Follows.Where(f => f.FollowerId == currentUserId && f.FolloweeId == userToUnfollow.Id)
            .ExecuteDeleteAsync();

        return new Profile
        (
            userToUnfollow.Username,
            userToUnfollow.Bio,
            userToUnfollow.Image,
            false
        );
    }
}
