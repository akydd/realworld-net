namespace realworld_net.Models;

public class User
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? Token { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }
}
