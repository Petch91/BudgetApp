using Entities.Contracts.Dtos;
using FluentResults;

namespace BudgetApp.Mobile.Services.Api;

public interface IMobileRapportService
{
    Task<Result<RapportMoisDto>> GetRapportAsync(int year, int month);
}
