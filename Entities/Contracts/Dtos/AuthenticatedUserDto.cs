namespace Entities.Contracts.Dtos;

public record AuthenticatedUserDto(
    int Id,
    string Username,
    string Email,
    string Token,
    string RefreshToken,
    DateTime ExpiresAt
);