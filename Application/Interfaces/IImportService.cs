using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Application.Interfaces;

/// <summary>
/// Import d'extraits de compte vers les transactions variables.
///
/// Le flux est volontairement en deux temps : <see cref="Analyser"/> ne fait qu'analyser
/// et proposer, <see cref="Confirmer"/> ecrit en base. Rien n'est insere sans validation
/// explicite de l'utilisateur.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Analyse un extrait : classe les mouvements, suggere des categories, detecte les
    /// doublons et les lignes correspondant deja a des depenses fixes. N'ecrit rien.
    /// </summary>
    Task<Result<ImportPreviewDto>> Analyser(Stream fichier, int userId);

    /// <summary>
    /// Insere les lignes validees par l'utilisateur, en ignorant celles deja importees.
    /// </summary>
    Task<Result<ImportResultDto>> Confirmer(ImportConfirmForm form, int userId);
}
