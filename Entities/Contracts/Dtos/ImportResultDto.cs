namespace Entities.Contracts.Dtos;

/// <summary>
/// Bilan d'un import confirme.
/// </summary>
public record ImportResultDto(
    int NbCreees,
    int NbIgnorees
);
