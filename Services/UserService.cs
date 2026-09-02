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

    public Task<User?> GetUserByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
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
}