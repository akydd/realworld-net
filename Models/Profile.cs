namespace realworld_net.Models;

public class Profile
{
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Image { get; set; }
    public bool Following { get; set; }
}
