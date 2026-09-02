namespace realworld_net.Services;

public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;

    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int userId)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT signing key 'JwtSettings:Secret' is not configured.");
        var key = System.Text.Encoding.ASCII.GetBytes(secret);
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("id", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT signing key 'JwtSettings:Secret' is not configured.");
        var key = System.Text.Encoding.ASCII.GetBytes(secret);
        try
        {
            tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (System.IdentityModel.Tokens.Jwt.JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "id");
            if (userIdClaim == null)
            {
                return null;
            }

            return int.Parse(userIdClaim.Value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            // Token validation failed
            return null;
        }
    }
}
