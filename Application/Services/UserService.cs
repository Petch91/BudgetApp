using Application.Interfaces;
using Application.Persistence;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(MyDbContext context) : IUserService
{
    public async Task<Result<UserDto>> GetById(int id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return Result.Fail("User not found");
        }

        return Result.Ok(new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.PasswordHash
        ));
    }

    public async Task<Result<UserDto>> GetByEmail(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Result.Fail("User not found");
        }

        return Result.Ok(new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.PasswordHash
        ));
    }

    public async Task<Result<User>> GetEntityByEmail(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Result.Fail("User not found");
        }

        return Result.Ok(user);
    }

    public async Task<Result> ExistsByEmail(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user != null ? Result.Ok() : Result.Fail("User not found");
    }

    public async Task<Result<UserDto>> Add(User user)
    {
        context.Users.Add(user);
        var result = await context.SaveChangesAsync();
        if (result == 0) return Result.Fail("User not added");
        return Result.Ok(new UserDto(user.Id, user.Username, user.Email, user.PasswordHash));
    }

    public async Task UpdatePasswordHash(int userId, string newHash)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) return;

        user.PasswordHash = newHash;
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByRefreshToken(string token)
    {
        return await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(rt =>
                    rt.Token == token &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<Result> RevokeRefreshToken(string token)
    {
        var rt = await context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        if (rt == null) return Result.Fail("Token not found");

        rt.IsRevoked = true;
        await context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task SaveChanges()
    {
        await context.SaveChangesAsync();
    }
}