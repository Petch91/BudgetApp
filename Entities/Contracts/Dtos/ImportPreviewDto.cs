namespace Entities.Contracts.Dtos;

/// <summary>
/// Resultat de l'analyse d'un extrait de compte : les lignes proposees et leur resume.
/// </summary>
public record ImportPreviewDto(
    IReadOnlyList<ImportedTransactionDto> Lignes,
    int TotalLignes,
    int NbDoublons,
    int NbDepensesFixes,
    int NbAImporter,
    decimal TotalDebits,
    decimal TotalCredits
);
