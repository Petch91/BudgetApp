using System.Security.Cryptography;
using Application.Interfaces;
using Application.Interfaces.Sécurité;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordManager;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(
        IUserService userService,
        IPasswordHasher passwordManager,
        IJwtTokenGenerator jwtGenerator)
    {
        _userService = userService;
        _passwordManager = passwordManager;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<AuthenticatedUserDto>> RegisterAsync(RegisterForm request)
    {
        var exists = await _userService.ExistsByEmail(request.Email);
        if (exists.IsSuccess)
            return Result.Fail<AuthenticatedUserDto>("Email déjà utilisé");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email
        };

        user.PasswordHash = _passwordManager.HashPassword(user, request.Password);

        var addResult = await _userService.Add(user);
        if (addResult.IsFailed)
            return Result.Fail<AuthenticatedUserDto>(addResult.Errors.First().Message);

        var accessToken = _jwtGenerator.GenerateToken(user, out var expiresAt);

        var refreshToken = CreateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        await _userService.SaveChanges();

        return Result.Ok(new AuthenticatedUserDto(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            refreshToken.Token,
            expiresAt
        ));
    }
    
    public async Task<Result<AuthenticatedUserDto>> LoginAsync(LoginForm request)
    {
        var userResult = await _userService.GetEntityByEmail(request.Email);
        if (userResult.IsFailed)
            return Result.Fail("Utilisateur inconnu");

        var user = userResult.Value;

        var verificationResult = _passwordManager.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
            return Result.Fail("Mot de passe invalide");

        // 🔁 Rehash automatique
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordManager.HashPassword(user, request.Password);
            await _userService.UpdatePasswordHash(user.Id, user.PasswordHash);
        }

        var accessToken = _jwtGenerator.GenerateToken(user, out var expiresAt);

        var refreshToken = CreateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        await _userService.SaveChanges();

        return Result.Ok(new AuthenticatedUserDto(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            refreshToken.Token,
            expiresAt
        ));
    }
    
    public async Task<Result<AuthenticatedUserDto>> RefreshToken(RefreshTokenForm request)
    {
        var user = await _userService.GetUserByRefreshToken(request.RefreshToken);
        if (user == null)
            return Result.Fail("Refresh token invalide");

        // 🔁 rotation
        var oldToken = user.RefreshTokens
            .First(rt => rt.Token == request.RefreshToken);

        oldToken.IsRevoked = true;

        var newRefresh = CreateRefreshToken();
        user.RefreshTokens.Add(newRefresh);

        var accessToken = _jwtGenerator.GenerateToken(user, out var expiresAt);

        await _userService.SaveChanges();

        return Result.Ok(new AuthenticatedUserDto(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            newRefresh.Token,
            expiresAt
        ));
    }
    
    public async Task<Result> Logout(string refreshToken)
    {
        var result = await _userService.RevokeRefreshToken(refreshToken);
        return result;
    }
    
    
    private RefreshToken CreateRefreshToken()
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
    }

}
