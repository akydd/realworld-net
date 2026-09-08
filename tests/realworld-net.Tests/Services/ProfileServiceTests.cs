using Microsoft.EntityFrameworkCore;
using realworld_net.Exceptions;
using realworld_net.Services;

namespace realworld_net.Tests.Services;

[Collection("Database")]
public class ProfileServiceTests : IAsyncLifetime
{
    private readonly DbFixture _dbFixture;

    public ProfileServiceTests(DbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    // ---- GetProfileByUsernameAsync ----

    [Fact]
    public async Task GetProfileByUsernameAsync_NoAuth_ReturnsProfileNotFollowing()
    {
        await SeedUserAsync("Joe", bio: "a bio", image: "img.png");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        var profile = await service.GetProfileByUsernameAsync("Joe", null);

        Assert.Equal("Joe", profile.Username);
        Assert.Equal("a bio", profile.Bio);
        Assert.Equal("img.png", profile.Image);
        Assert.False(profile.Following);
    }

    [Fact]
    public async Task GetProfileByUsernameAsync_Auth_ReturnsFollowingTrueWhenFollowing()
    {
        var target = await SeedUserAsync("Joe");
        var follower = await SeedUserAsync("Mo");
        await SeedFollowsAsync(follower.Id, target.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        var profile = await service.GetProfileByUsernameAsync("Joe", follower.Id);

        Assert.Equal("Joe", profile.Username);
        Assert.True(profile.Following);
    }

    [Fact]
    public async Task GetProfileByUsernameAsync_Auth_ReturnsFollowingFalseWhenNotFollowing()
    {
        await SeedUserAsync("Joe");
        var user = await SeedUserAsync("Mo");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        var profile = await service.GetProfileByUsernameAsync("Joe", user.Id);

        Assert.False(profile.Following);
    }

    [Fact]
    public async Task GetProfileByUsernameAsync_ThrowsWhenUserNotFound()
    {
        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await service.GetProfileByUsernameAsync("ghost", null));
    }

    // ---- FollowUserAsync ----

    [Fact]
    public async Task FollowUserAsync_CreatesFollowAndReturnsFollowing()
    {
        var target = await SeedUserAsync("Joe");
        var follower = await SeedUserAsync("Mo");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        var profile = await service.FollowUserAsync("Joe", follower.Id);

        Assert.Equal("Joe", profile.Username);
        Assert.True(profile.Following);

        await using var assertContext = _dbFixture.CreateContext();
        var followExists = await assertContext.Follows
            .AnyAsync(f => f.FollowerId == follower.Id && f.FolloweeId == target.Id);
        Assert.True(followExists);
    }

    [Fact]
    public async Task FollowUserAsync_Idempotent()
    {
        var target = await SeedUserAsync("Joe");
        var follower = await SeedUserAsync("Mo");

        // Each call uses its own context, mirroring two separate HTTP requests
        // (the app resolves a scoped DbContext per request). Following twice must
        // not error and must not create a second row.
        await using (var context1 = _dbFixture.CreateContext())
        {
            await new ProfileService(context1).FollowUserAsync("Joe", follower.Id);
        }

        await using (var context2 = _dbFixture.CreateContext())
        {
            var profile = await new ProfileService(context2).FollowUserAsync("Joe", follower.Id);
            Assert.True(profile.Following);
        }

        await using var assertContext = _dbFixture.CreateContext();
        var followCount = await assertContext.Follows
            .CountAsync(f => f.FollowerId == follower.Id && f.FolloweeId == target.Id);
        Assert.Equal(1, followCount);
    }

    [Fact]
    public async Task FollowUserAsync_ThrowsWhenUserNotFound()
    {
        var follower = await SeedUserAsync("Mo");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await service.FollowUserAsync("ghost", follower.Id));
    }

    // ---- UnfollowUserAsync ----

    [Fact]
    public async Task UnfollowUserAsync_RemovesFollowAndReturnsNotFollowing()
    {
        var target = await SeedUserAsync("Joe");
        var follower = await SeedUserAsync("Mo");
        await SeedFollowsAsync(follower.Id, target.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        var profile = await service.UnfollowUserAsync("Joe", follower.Id);

        Assert.Equal("Joe", profile.Username);
        Assert.False(profile.Following);

        await using var assertContext = _dbFixture.CreateContext();
        var followExists = await assertContext.Follows
            .AnyAsync(f => f.FollowerId == follower.Id && f.FolloweeId == target.Id);
        Assert.False(followExists);
    }

    [Fact]
    public async Task UnfollowUserAsync_NoOpWhenNotFollowing()
    {
        var target = await SeedUserAsync("Joe");
        var follower = await SeedUserAsync("Mo");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        // Unfollowing when not following is a safe no-op, not an error.
        var profile = await service.UnfollowUserAsync("Joe", follower.Id);
        Assert.False(profile.Following);

        await using var assertContext = _dbFixture.CreateContext();
        var followExists = await assertContext.Follows
            .AnyAsync(f => f.FollowerId == follower.Id && f.FolloweeId == target.Id);
        Assert.False(followExists);
    }

    [Fact]
    public async Task UnfollowUserAsync_ThrowsWhenUserNotFound()
    {
        var follower = await SeedUserAsync("Mo");

        await using var context = _dbFixture.CreateContext();
        var service = new ProfileService(context);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await service.UnfollowUserAsync("ghost", follower.Id));
    }

    public Task DisposeAsync() => Task.CompletedTask;
    public async Task InitializeAsync() => await _dbFixture.ResetAsync();

    private async Task<Entities.User> SeedUserAsync(string username = "Joe", string? bio = null, string? image = null)
    {
        await using var context = _dbFixture.CreateContext();
        var user = new Entities.User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = "password",
            Bio = bio,
            Image = image,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Entities.Follows> SeedFollowsAsync(int followerId, int followeeId)
    {
        await using var context = _dbFixture.CreateContext();
        var follows = new Entities.Follows
        {
            FollowerId = followerId,
            FolloweeId = followeeId,
        };
        context.Follows.Add(follows);
        await context.SaveChangesAsync();
        return follows;
    }
}
