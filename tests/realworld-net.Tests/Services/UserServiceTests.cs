using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Dtos;
using realworld_net.Exceptions;
using realworld_net.Services;

namespace realworld_net.Tests.Services;

[Collection("Database")]
public class UserServiceTests : IAsyncLifetime
{
    private readonly DbFixture _dbFixture;
    private readonly PasswordHasher _passwordHasher = new();
    public Task DisposeAsync() => Task.CompletedTask;
    public Task InitializeAsync() => _dbFixture.ResetAsync();

    public UserServiceTests(DbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    private UserService CreateUserService(AppDbContext context)
    {
        return new UserService(context, _passwordHasher, new TestJwtService());
    }

    [Fact]
    public async Task GetUserByIdAsync_SucceedsForExistingUser()
    {
        await using var context = _dbFixture.CreateContext();
        var user = await SeedUser();

        var service = CreateUserService(context);
        var fetchedUser = await service.GetUserByIdAsync(user.Id);
        Assert.NotNull(fetchedUser);
        Assert.Equal(user.Bio, fetchedUser.Bio);
        Assert.Equal(user.Email, fetchedUser.Email);
        Assert.Equal(user.Image, fetchedUser.Image);
        Assert.Equal(user.Username, fetchedUser.Username);
        Assert.Null(fetchedUser.Token);
    }

    private async Task<Entities.User> SeedUser(string username = "Bob", string password = "password")
    {
        await using var context = _dbFixture.CreateContext();
        var user = new Entities.User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = _passwordHasher.HashPassword(password),
            Image = "testimage.png",
            Bio = "Test bio."
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task RegisterUserAsync_SucceedsForNewUser()
    {
        await using var context = _dbFixture.CreateContext();
        var regUser = new RegisterUserDto(new RegisterUserInnerDto("Joe", "joe@test.com", "password"));
        var service = CreateUserService(context);

        var newUser = await service.RegisterUserAsync(regUser);
        Assert.NotNull(newUser);
        Assert.Equal(regUser.User.Email, newUser.Email);
        Assert.Equal(regUser.User.Username, newUser.Username);
        Assert.Null(newUser.Bio);
        Assert.Null(newUser.Image);
        Assert.NotNull(newUser.Token);

        await using var assertContext = _dbFixture.CreateContext();
        var foundUser = await assertContext.Users.SingleAsync(u => u.Username == regUser.User.Username);
        Assert.NotNull(foundUser);
        Assert.Equal(regUser.User.Email, foundUser.Email);
        Assert.Equal(regUser.User.Username, foundUser.Username);
        Assert.Null(foundUser.Bio);
        Assert.Null(foundUser.Image);
        Assert.True(_passwordHasher.VerifyPassword(foundUser.PasswordHash, regUser.User.Password));

        Assert.Equal($"{foundUser.Id}-token", newUser.Token);
    }

    [Fact]
    public async Task LoginUser_SucceedsWithCorrectPassword()
    {
        await using var context = _dbFixture.CreateContext();
        const string password = "password";
        var user = await SeedUser(password: password);

        var service = CreateUserService(context);
        var loginDto = new LoginUserDto(new LoginUserInnerDto(user.Email, password));
        var loggedInUser = await service.LoginUserAsync(loginDto);
        Assert.NotNull(loggedInUser);
        Assert.Equal(user.Bio, loggedInUser.Bio);
        Assert.Equal(user.Email, loggedInUser.Email);
        Assert.Equal(user.Image, loggedInUser.Image);
        Assert.Equal(user.Username, loggedInUser.Username);
        Assert.Equal($"{user.Id}-token", loggedInUser.Token);
    }

    [Fact]
    public async Task LoginUser_FailsWithIncorrectPassword()
    {
        await using var context = _dbFixture.CreateContext();
        const string password = "password";
        var user = await SeedUser(password: password);

        var service = CreateUserService(context);
        var loginDto = new LoginUserDto(new LoginUserInnerDto(user.Email, "not the password"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await service.LoginUserAsync(loginDto));
    }

    [Fact]
    public async Task UpdateUser_SucceedsForUser()
    {
        await using var context = _dbFixture.CreateContext();
        var user = await SeedUser();

        var service = CreateUserService(context);
        var updateDto = new UpdateUserDto(new UpdateUserInnerDto("newemail@test.com", "newuser", "newpassword", "new bio", "new image"));
        var updatedUser = await service.UpdateUserAsync(user.Id, updateDto);
        Assert.NotNull(updatedUser);
        Assert.Equal(updateDto.User.Email, updatedUser.Email);
        Assert.Equal(updateDto.User.Bio, updatedUser.Bio);
        Assert.Equal(updateDto.User.Image, updatedUser.Image);
        Assert.Equal(updateDto.User.Username, updatedUser.Username);

        await using var assertContext = _dbFixture.CreateContext();
        var foundUser = await assertContext.Users.SingleAsync(u => u.Id == user.Id);
        Assert.NotNull(foundUser);
        Assert.Equal(updateDto.User.Email, foundUser.Email);
        Assert.Equal(updateDto.User.Bio, foundUser.Bio);
        Assert.Equal(updateDto.User.Image, foundUser.Image);
        Assert.Equal(updateDto.User.Username, foundUser.Username);
        Assert.True(_passwordHasher.VerifyPassword(foundUser.PasswordHash, updateDto.User.Password!));
    }

    [Fact]
    public async Task GetUserByIdAsync_ThrowsWhenUserNotFound()
    {
        await using var context = _dbFixture.CreateContext();
        var service = CreateUserService(context);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await service.GetUserByIdAsync(999));
    }

    [Fact]
    public async Task LoginUser_FailsForUnknownEmail()
    {
        await using var context = _dbFixture.CreateContext();
        var service = CreateUserService(context);

        var loginDto = new LoginUserDto(new LoginUserInnerDto("nobody@test.com", "password"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.LoginUserAsync(loginDto));
    }

    [Fact]
    public async Task RegisterUserAsync_ThrowsForDuplicateUsername()
    {
        await SeedUser("Joe");

        await using var context = _dbFixture.CreateContext();
        var service = CreateUserService(context);

        // Same username, different email — isolates the Username unique constraint.
        var regUser = new RegisterUserDto(new RegisterUserInnerDto("Joe", "different@test.com", "password"));
        await Assert.ThrowsAsync<UniqueConstraintException>(async () =>
            await service.RegisterUserAsync(regUser));
    }

    [Fact]
    public async Task UpdateUser_ThrowsWhenUserNotFound()
    {
        await using var context = _dbFixture.CreateContext();
        var service = CreateUserService(context);

        var updateDto = new UpdateUserDto(new UpdateUserInnerDto(null, null, null, "new bio", null));
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await service.UpdateUserAsync(999, updateDto));
    }

    [Fact]
    public async Task UpdateUser_PartialUpdateLeavesOtherFieldsUnchanged()
    {
        await using var context = _dbFixture.CreateContext();
        var user = await SeedUser();

        var service = CreateUserService(context);
        // Only Bio is set; every other field is null and must be left untouched.
        var updateDto = new UpdateUserDto(new UpdateUserInnerDto(null, null, null, "updated bio", null));
        var updatedUser = await service.UpdateUserAsync(user.Id, updateDto);

        Assert.Equal("updated bio", updatedUser.Bio);
        Assert.Equal(user.Username, updatedUser.Username);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Equal(user.Image, updatedUser.Image);

        await using var assertContext = _dbFixture.CreateContext();
        var foundUser = await assertContext.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal("updated bio", foundUser.Bio);
        Assert.Equal(user.Username, foundUser.Username);
        Assert.Equal(user.Email, foundUser.Email);
        Assert.Equal(user.Image, foundUser.Image);
        // Password was not provided, so the original hash must still verify.
        Assert.True(_passwordHasher.VerifyPassword(foundUser.PasswordHash, "password"));
    }
}

sealed class TestJwtService : IJWTService
{
    public string GenerateToken(int userId) => $"{userId}-token";
}
