using Entities.Contracts.Dtos;
using FluentResults;

namespace BudgetApp.Mobile.Services.Api;

public interface IMobileCategorieService
{
    Task<Result<List<CategorieDto>>> GetAllAsync();
}
