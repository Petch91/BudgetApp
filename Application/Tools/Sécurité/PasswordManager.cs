using Application.Interfaces.Sécurité;
using Entities.Contracts.Dtos;
using Entities.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Tools.Sécurité;

public class PasswordManager : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result;
    }
}
