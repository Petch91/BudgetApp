using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces.Sécurité;
using Entities.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Application.Tools.Sécurité;

public class JwtOptions
{
    public string Secret { get; set; } = "kjnbdsfqjhfuhsqioFJHIOHFZEHFBIJHBVIDSBJHVBDJHSBVHD76786!ç'yfy9TTtsdètcsdgyçé!é98656";
    public string Issuer { get; set; } = "BudgetApp";
    public string Audience { get; set; } = "BudgetAppUsers";
    public int ExpirationMinutes { get; set; } = 60 * 24 * 7; // 7 jours
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly byte[] _key;

    public JwtTokenGenerator(JwtOptions options)
    {
        _options = options;
        _key = Encoding.UTF8.GetBytes(_options.Secret);
    }

    public string GenerateToken(User user, out DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
        };

        expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
