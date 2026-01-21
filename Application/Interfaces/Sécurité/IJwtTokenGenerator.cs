using Entities.Contracts.Dtos;
using Entities.Domain.Models;

namespace Application.Interfaces.Sécurité;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, out DateTime expiresAt);
}