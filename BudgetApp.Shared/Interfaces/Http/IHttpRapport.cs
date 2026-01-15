using Entities.Contracts.Dtos;
using FluentResults;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpRapport
{
    Task<Result<RapportMoisDto>> GetRapportMois(int annee, int mois);
}
