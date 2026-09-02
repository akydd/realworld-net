using realworld_net.Dtos;
using realworld_net.Models;

namespace realworld_net.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> RegisterUserAsync(RegisterUserDto userDto);
}
