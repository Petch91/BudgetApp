using Entities.Contracts.Dtos;
using FluentResults;

namespace Application.Interfaces;

public interface IRapportService
{
    Task<Result<RapportMoisDto>> GetRapportMois(int annee, int mois, int userId);

    /// <summary>
    /// Liste des dépenses fixes qui tombent sur le mois donné : occurrences récurrentes
    /// (mensuel/trimestriel/biannuel/annuel dont une échéance tombe ce mois) + échéances
    /// échelonnées prévues ce mois. Pour l'export CSV.
    /// </summary>
    Task<Result<IReadOnlyList<DepenseFixeMoisDto>>> GetDepensesFixesMois(int annee, int mois, int userId);
}
