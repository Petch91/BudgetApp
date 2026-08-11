using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpImport
{
    /// <summary>Envoie l'extrait au serveur pour analyse. N'ecrit rien en base.</summary>
    Task<Result<ImportPreviewDto>> Analyser(Stream fichier, string nomFichier);

    /// <summary>Confirme l'import des lignes selectionnees.</summary>
    Task<Result<ImportResultDto>> Confirmer(ImportConfirmForm form);
}
