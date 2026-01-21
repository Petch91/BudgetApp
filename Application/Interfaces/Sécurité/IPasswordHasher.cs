using Entities.Contracts.Dtos;
using Entities.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Sécurité;

public interface IPasswordHasher
{
    string HashPassword(User user, string password);
    PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword);
}