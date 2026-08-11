using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.Persistence;
using Application.Services.Import;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Services;

public partial class ImportService(MyDbContext context, IBankStatementParser parser) : IImportService
{
    private const int IntituleMaxLength = 150;

    /// <summary>Types qui correspondent presque toujours a une depense fixe deja enregistree.</summary>
    private static readonly HashSet<MouvementBancaireType> TypesDepenseFixe =
    [
        MouvementBancaireType.Domiciliation,
        MouvementBancaireType.OrdrePermanent,
        MouvementBancaireType.Credit
    ];

    /* =======================
     * ANALYSE (aucune ecriture)
     * ======================= */

    public async Task<Result<ImportPreviewDto>> Analyser(Stream fichier, int userId)
    {
        Log.Information("Analyse d'un extrait {Format} pour userId {UserId}", parser.FormatName, userId);

        var parsing = parser.Parse(fichier);
        if (parsing.IsFailed)
            return Result.Fail(parsing.Errors);

        var mouvements = parsing.Value;

        var categories = await context.Categories
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        if (categories.Count == 0)
        {
            Log.Warning("Import impossible : aucune categorie en base");
            return Result.Fail("Aucune categorie disponible : creez-en une avant d'importer");
        }

        var categorieParNom = new Dictionary<string, int>();
        foreach (var c in categories)
            categorieParNom.TryAdd(Normaliser(c.Name), c.Id);

        var categorieParId = categories.ToDictionary(c => c.Id, c => c.Name);
        var categorieDefautId = categories.Any(c => c.Id == 1) ? 1 : categories[0].Id;

        // Deja importe ? On compare sur la reference bancaire du mouvement.
        var refsExistantes = await context.TransactionsVariables
            .Where(t => t.UserId == userId && t.ExternalRef != null)
            .Select(t => t.ExternalRef!)
            .ToListAsync();

        var refsConnues = refsExistantes.ToHashSet(StringComparer.Ordinal);

        var lignes = new List<ImportedTransactionDto>(mouvements.Count);

        foreach (var mouvement in mouvements)
        {
            var estDepenseFixe = TypesDepenseFixe.Contains(mouvement.Type);
            var estDoublon = refsConnues.Contains(mouvement.ExternalRef);

            var nomSuggere = CategorieMatcher.Suggerer(mouvement);
            var categorieId = nomSuggere is not null
                              && categorieParNom.TryGetValue(Normaliser(nomSuggere), out var id)
                ? id
                : categorieDefautId;

            lignes.Add(new ImportedTransactionDto(
                ExternalRef: mouvement.ExternalRef,
                Date: mouvement.DateValeur,
                Intitule: ConstruireIntitule(mouvement),
                Description: mouvement.Description,
                Montant: Math.Abs(mouvement.Montant),
                IsRevenu: mouvement.IsRevenu,
                Type: mouvement.Type,
                TypeLabel: LabelType(mouvement.Type),
                CategorieId: categorieId,
                CategorieName: categorieParId.GetValueOrDefault(categorieId, string.Empty),
                IsDuplicate: estDoublon,
                IsLikelyDepenseFixe: estDepenseFixe,
                // Par defaut on n'importe que ce qui est nouveau et pas deja couvert par une depense fixe.
                Include: !estDoublon && !estDepenseFixe
            ));
        }

        var preview = new ImportPreviewDto(
            Lignes: lignes,
            TotalLignes: lignes.Count,
            NbDoublons: lignes.Count(l => l.IsDuplicate),
            NbDepensesFixes: lignes.Count(l => l.IsLikelyDepenseFixe),
            NbAImporter: lignes.Count(l => l.Include),
            TotalDebits: lignes.Where(l => l is { Include: true, IsRevenu: false }).Sum(l => l.Montant),
            TotalCredits: lignes.Where(l => l is { Include: true, IsRevenu: true }).Sum(l => l.Montant)
        );

        Log.Information(
            "Analyse terminee : {Total} mouvement(s), {Doublons} doublon(s), {Fixes} depense(s) fixe(s), {AImporter} a importer",
            preview.TotalLignes, preview.NbDoublons, preview.NbDepensesFixes, preview.NbAImporter);

        return Result.Ok(preview);
    }

    /* =======================
     * CONFIRMATION (ecriture)
     * ======================= */

    public async Task<Result<ImportResultDto>> Confirmer(ImportConfirmForm form, int userId)
    {
        if (form.Lignes.Count == 0)
            return Result.Fail("Aucune ligne selectionnee");

        Log.Information("Import de {Count} ligne(s) pour userId {UserId}", form.Lignes.Count, userId);

        var categorieIds = await context.Categories
            .Select(c => c.Id)
            .ToListAsync();

        if (categorieIds.Count == 0)
            return Result.Fail("Aucune categorie disponible");

        var categorieDefautId = categorieIds.Contains(1) ? 1 : categorieIds[0];

        // Re-verification cote serveur : l'apercu a pu vieillir entre l'analyse et la validation.
        var refsExistantes = await context.TransactionsVariables
            .Where(t => t.UserId == userId && t.ExternalRef != null)
            .Select(t => t.ExternalRef!)
            .ToListAsync();

        var refsConnues = refsExistantes.ToHashSet(StringComparer.Ordinal);

        var aCreer = new List<TransactionVariable>();
        var ignorees = 0;

        foreach (var ligne in form.Lignes)
        {
            // Doublon deja en base, ou doublon a l'interieur du meme envoi.
            if (!string.IsNullOrWhiteSpace(ligne.ExternalRef) && !refsConnues.Add(ligne.ExternalRef))
            {
                ignorees++;
                continue;
            }

            var intitule = string.IsNullOrWhiteSpace(ligne.Intitule)
                ? "Mouvement bancaire"
                : ligne.Intitule.Trim();

            aCreer.Add(new TransactionVariable
            {
                Intitule = Tronquer(intitule, IntituleMaxLength),
                Montant = Math.Abs(ligne.Montant),
                Date = ligne.Date,
                TransactionType = ligne.IsRevenu ? TransactionType.Revenu : TransactionType.Depense,
                CategorieId = categorieIds.Contains(ligne.CategorieId) ? ligne.CategorieId : categorieDefautId,
                UserId = userId,
                ExternalRef = string.IsNullOrWhiteSpace(ligne.ExternalRef) ? null : ligne.ExternalRef
            });
        }

        if (aCreer.Count > 0)
        {
            context.TransactionsVariables.AddRange(aCreer);
            await context.SaveChangesAsync();
        }

        Log.Information("Import termine : {Creees} creee(s), {Ignorees} ignoree(s)", aCreer.Count, ignorees);

        return Result.Ok(new ImportResultDto(aCreer.Count, ignorees));
    }

    /* =======================
     * PRIVATE LOGIC
     * ======================= */

    /// <summary>
    /// L'intitule affiche : le marchand quand la banque le fournit, sinon on l'extrait
    /// du libelle selon la forme du mouvement.
    /// </summary>
    private static string ConstruireIntitule(MouvementBancaire mouvement)
    {
        if (!string.IsNullOrWhiteSpace(mouvement.NomContrepartie))
            return Tronquer(mouvement.NomContrepartie.Trim(), IntituleMaxLength);

        var description = mouvement.Description;

        // "BANCONTACT - ACHAT - LIDL 418 AYWAILLE - 4920 AYWAILLE BE - ..."
        if (description.StartsWith("BANCONTACT", StringComparison.OrdinalIgnoreCase))
        {
            var segments = description.Split(" - ", StringSplitOptions.TrimEntries);
            if (segments.Length >= 3)
                return Tronquer(NettoyerLibelle(segments[2]), IntituleMaxLength);
        }

        // Pour ces trois formes, le libelle utile suit " POUR " :
        //   "VOTRE DOMICILIATION EUROPEENNE <id> POUR <beneficiaire> COMMUNICATION : ..."
        //   "ORDRE PERMANENT <id> POUR <iban> <beneficiaire>"
        //   "PAIEMENTS DUS POUR VOTRE CREDIT LOGEMENT <iban> REF. : ..."
        // On ne l'applique qu'a ces types : ailleurs, " POUR " apparait en plein milieu
        // d'une phrase et tronquerait le libelle n'importe ou.
        if (mouvement.Type is MouvementBancaireType.Domiciliation
                           or MouvementBancaireType.OrdrePermanent
                           or MouvementBancaireType.Credit)
        {
            var indexPour = description.IndexOf(" POUR ", StringComparison.OrdinalIgnoreCase);
            if (indexPour >= 0)
            {
                var beneficiaire = NettoyerLibelle(description[(indexPour + 6)..]);
                if (beneficiaire.Length > 0)
                    return Tronquer(beneficiaire, IntituleMaxLength);
            }
        }

        return Tronquer(NettoyerLibelle(description), IntituleMaxLength);
    }

    /// <summary>
    /// Marqueurs a partir desquels le libelle bancaire ne contient plus que de la
    /// plomberie : reference d'operation, reference creancier, communication, date valeur.
    /// </summary>
    private static readonly string[] MarqueursTechniques =
    [
        "REF. :", "REF.:", "REFERENCE DU CREANCIER", "COMMUNICATION :", "COMMUNICATION:", " VAL. "
    ];

    /// <summary>
    /// Retire la queue technique et les IBAN d'un libelle, pour n'en garder que la partie
    /// lisible. "VOTRE CREDIT LOGEMENT BE83 0753 5141 5415 REF. : 0800181128802 VAL. 01-08"
    /// devient "VOTRE CREDIT LOGEMENT".
    /// </summary>
    private static string NettoyerLibelle(string libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle))
            return string.Empty;

        var texte = libelle;

        foreach (var marqueur in MarqueursTechniques)
        {
            var index = texte.IndexOf(marqueur, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
                texte = texte[..index];
        }

        // Un IBAN au milieu du libelle n'apporte rien a la lecture.
        var sansIban = IbanRegex().Replace(texte, " ");

        // On ne garde le nettoyage que s'il reste quelque chose de lisible.
        var candidat = EspacesMultiplesRegex().Replace(sansIban, " ").Trim(' ', '-', ',', ':', ';');

        return candidat.Length > 0
            ? candidat
            : EspacesMultiplesRegex().Replace(texte, " ").Trim();
    }

    [GeneratedRegex(@"\b[A-Z]{2}\d{2}(?:\s?[A-Z0-9]{4}){2,7}\b")]
    private static partial Regex IbanRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacesMultiplesRegex();

    private static string LabelType(MouvementBancaireType type) => type switch
    {
        MouvementBancaireType.AchatCarte => "Achat carte",
        MouvementBancaireType.Cash => "Retrait",
        MouvementBancaireType.Virement => "Virement",
        MouvementBancaireType.Versement => "Versement",
        MouvementBancaireType.Remboursement => "Remboursement",
        MouvementBancaireType.Domiciliation => "Domiciliation",
        MouvementBancaireType.OrdrePermanent => "Ordre permanent",
        MouvementBancaireType.Credit => "Crédit",
        MouvementBancaireType.Frais => "Frais bancaires",
        MouvementBancaireType.Interet => "Intérêts",
        _ => "Autre"
    };

    private static string Tronquer(string valeur, int max)
        => valeur.Length <= max ? valeur : valeur[..max];

    /// <summary>Minuscules sans accents, pour comparer les noms de categorie.</summary>
    private static string Normaliser(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
            return string.Empty;

        var decompose = valeur.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decompose.Length);

        foreach (var c in decompose)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
