using Entities.Contracts.Dtos;
using FluentResults;

namespace Application.Interfaces;

public interface IRapportService
{
    Task<Result<RapportMoisDto>> GetRapportMois(int annee, int mois);
}
