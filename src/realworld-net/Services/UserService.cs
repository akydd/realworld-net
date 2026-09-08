using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Dtos;
using realworld_net.Exceptions;
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

    public async Task<User> GetUserByIdAsync(int userId)
    {
        // This is unlikely when called from the Controller, but we handle it anyway.
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId) ?? throw new NotFoundException("user");
        return new User
            (
                user.Username,
                user.Email,
                null,
                user.Bio,
                user.Image
            );
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
        (
            newUser.Username,
            newUser.Email,
            token,
            null,
            null
        );
    }

    public async Task<User> LoginUserAsync(LoginUserDto userDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.User.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, userDto.User.Password))
        {
            throw new UnauthorizedException("credentials", "invalid");
        }

        var token = _jwtService.GenerateToken(user.Id);

        return new User
        (
            user.Username,
            user.Email,
            token,
            user.Bio,
            user.Image
        );
    }

    public async Task<User> UpdateUserAsync(int userId, UpdateUserDto userDto)
    {
        var innerDto = userDto.User;
        var userToUpdate = await _context.Users.FindAsync(userId) ?? throw new NotFoundException("user");

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
        (
            userToUpdate.Username,
            userToUpdate.Email,
            null,
            userToUpdate.Bio,
            userToUpdate.Image
        );
    }
}
