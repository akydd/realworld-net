namespace realworld_net.Services;

public interface IJWTService
{
    string GenerateToken(int userId);
    int? ValidateToken(string token);

}
