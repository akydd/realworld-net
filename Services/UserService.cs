using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Dtos;
using realworld_net.Models;
using DbUser = realworld_net.Entities.User;

namespace realworld_net.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    private readonly IJWTService _jwtService;

    public UserService(AppDbContext context, IPasswordHasher passwordHasher, IJWTService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null
            ? throw new UnauthorizedAccessException("User not found.")
            : new User
            {
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                Image = user.Image
            };
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user == null
            ? throw new UnauthorizedAccessException("User not found.")
            : new User
            {
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                Image = user.Image
            };
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<User> RegisterUserAsync(RegisterUserDto userDto)
    {
        var hashedPassword = _passwordHasher.HashPassword(userDto.User.Password);
        var newUser = new DbUser
        {
            Username = userDto.User.Username,
            Email = userDto.User.Email,
            PasswordHash = hashedPassword
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(newUser.Id);

        return new User
        {
            Username = newUser.Username,
            Email = newUser.Email,
            Token = token
        };
    }

    public async Task<User> LoginUserAsync(LoginUserDto userDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.User.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, userDto.User.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtService.GenerateToken(user.Id);

        return new User
        {
            Username = user.Username,
            Email = user.Email,
            Token = token,
            Bio = user.Bio,
            Image = user.Image
        };
    }

    public async Task<User> UpdateUserAsync(int userId, UpdateUserDto userDto)
    {
        var innerDto = userDto.User;
        var userToUpdate = await _context.Users.FindAsync(userId) ?? throw new UnauthorizedAccessException("User not found.");

        if (innerDto.Bio != null)
        {
            userToUpdate.Bio = innerDto.Bio;
        }
        if (innerDto.Image != null)
        {
            userToUpdate.Image = innerDto.Image;
        }
        if (innerDto.Username != null)
        {
            userToUpdate.Username = innerDto.Username;
        }
        if (innerDto.Email != null)
        {
            userToUpdate.Email = innerDto.Email;
        }
        if (innerDto.Password != null)
        {
            userToUpdate.PasswordHash = _passwordHasher.HashPassword(innerDto.Password);
        }

        await _context.SaveChangesAsync();

        return new User
        {
            Username = userToUpdate.Username,
            Email = userToUpdate.Email,
            Bio = userToUpdate.Bio,
            Image = userToUpdate.Image
        };
    }
}
