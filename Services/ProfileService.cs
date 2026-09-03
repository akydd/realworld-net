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
        {
            Username = user.Username,
            Bio = user.Bio,
            Image = user.Image,
            Following = isFollowing
        };
    }

    public async Task<Profile> FollowUserAsync(string username, int currentUserId)
    {
        var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");

        if (userToFollow.Id == currentUserId)
        {
            throw new Exception("You cannot follow yourself.");
        }

        var existingFollow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == userToFollow.Id) ?? throw new Exception("You are already following this user.");
        if (existingFollow != null)
        {
            throw new Exception("You are already following this user.");
        }

        var follow = new Entities.Follows
        {
            FollowerId = currentUserId,
            FolloweeId = userToFollow.Id
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        return new Profile
        {
            Username = userToFollow.Username,
            Bio = userToFollow.Bio,
            Image = userToFollow.Image,
            Following = true
        };
    }

    public async Task<Profile> UnfollowUserAsync(string username, int currentUserId)
    {
        var userToUnfollow = await _context.Users.FirstOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");

        if (userToUnfollow.Id == currentUserId)
        {
            throw new Exception("You cannot unfollow yourself.");
        }

        var existingFollow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == userToUnfollow.Id) ?? throw new Exception("You are not following this user.");
        _context.Follows.Remove(existingFollow);
        await _context.SaveChangesAsync();

        return new Profile
        {
            Username = userToUnfollow.Username,
            Bio = userToUnfollow.Bio,
            Image = userToUnfollow.Image,
            Following = false
        };
    }
}
