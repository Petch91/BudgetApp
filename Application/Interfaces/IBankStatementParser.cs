using Entities.Domain.Models;
using FluentResults;

namespace Application.Interfaces;

/// <summary>
/// Lit un extrait de compte et le transforme en mouvements bancaires bruts.
/// Une implementation par format bancaire.
/// </summary>
public interface IBankStatementParser
{
    /// <summary>Nom du format supporte, pour les logs et messages utilisateur.</summary>
    string FormatName { get; }

    /// <summary>
    /// Parse le flux fourni. Ne fait aucun acces base et n'applique aucune regle metier.
    /// </summary>
    Result<IReadOnlyList<MouvementBancaire>> Parse(Stream stream);
}
