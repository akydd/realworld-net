using Microsoft.EntityFrameworkCore;

namespace realworld_net.Entities;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }

    public string PasswordHash { get; set; }
}