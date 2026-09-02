using Microsoft.AspNetCore.Identity;

namespace realworld_net.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }
}
