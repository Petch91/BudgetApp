using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Application.Interfaces.Sécurité;

public interface IAuthService
{
        Task<Result<AuthenticatedUserDto>> RegisterAsync(RegisterForm request);
        Task<Result<AuthenticatedUserDto>> LoginAsync(LoginForm request);
        Task<Result<AuthenticatedUserDto>> RefreshToken(RefreshTokenForm request);
        Task<Result> Logout(string refreshToken);
}