using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> GetById(int id);

    Task<Result<UserDto>> GetByEmail(string email);
    Task<Result<User>> GetEntityByEmail(string email);


    Task<Result> ExistsByEmail(string email);

    Task<Result<UserDto>> Add(User user);
    
    Task UpdatePasswordHash(int userId, string newHash);
    
    Task<User?> GetUserByRefreshToken(string token);
    Task SaveChanges();
    Task<Result> RevokeRefreshToken(string token);



    //Task<Result<User>> Update();
}