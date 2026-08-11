using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Entities.Domain.Models;
using FluentResults;
using Serilog;

namespace Application.Services.Import;

/// <summary>
/// Parser des exports CSV Belfius (web banking).
///
/// Particularites du format :
/// - encodage Windows-1252 (accents casses si lu en UTF-8) ;
/// - separateur point-virgule ;
/// - une dizaine de lignes d'en-tete (filtres, dernier solde) avant la vraie ligne de colonnes ;
/// - montants signes a virgule decimale et point comme separateur de milliers ;
/// - la reference bancaire du mouvement est noyee dans le libelle, sous la forme "REF. : xxx VAL. jj-mm".
/// </summary>
public partial class BelfiusCsvParser : IBankStatementParser
{
    public string FormatName => "Belfius CSV";

    private static readonly NumberFormatInfo MontantFormat = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = "."
    };

    static BelfiusCsvParser()
    {
        // Windows-1252 n'est pas disponible par defaut sur .NET Core.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Result<IReadOnlyList<MouvementBancaire>> Parse(Stream stream)
    {
        List<string> lignes;

        try
        {
            using var reader = new StreamReader(stream, Encoding.GetEncoding(1252));
            lignes = [];
            while (reader.ReadLine() is { } ligne)
                lignes.Add(ligne);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lecture impossible du fichier CSV Belfius");
            return Result.Fail("Impossible de lire le fichier");
        }

        var headerIndex = TrouverLigneEntetes(lignes);
        if (headerIndex < 0)
        {
            Log.Warning("Aucune ligne d'entetes trouvee dans le CSV ({Count} lignes)", lignes.Count);
            return Result.Fail("Ce fichier ne ressemble pas a un export CSV Belfius (entetes introuvables)");
        }

        var colonnes = MapperColonnes(SplitCsv(lignes[headerIndex]));

        var mouvements = new List<MouvementBancaire>();
        var ignorees = 0;

        for (var i = headerIndex + 1; i < lignes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lignes[i]))
                continue;

            var mouvement = ParserLigne(SplitCsv(lignes[i]), colonnes);
            if (mouvement is null)
            {
                ignorees++;
                continue;
            }

            mouvements.Add(mouvement);
        }

        if (ignorees > 0)
            Log.Warning("{Count} ligne(s) du CSV ignoree(s) car illisibles", ignorees);

        if (mouvements.Count == 0)
            return Result.Fail("Aucun mouvement exploitable dans ce fichier");

        Log.Information("CSV Belfius parse : {Count} mouvement(s)", mouvements.Count);
        return Result.Ok<IReadOnlyList<MouvementBancaire>>(mouvements);
    }

    /* =======================
     * STRUCTURE DU FICHIER
     * ======================= */

    /// <summary>
    /// La vraie ligne de colonnes est la premiere dont le premier champ vaut "Compte"
    /// et qui contient assez de colonnes. Les lignes de filtres au-dessus sont ignorees.
    /// </summary>
    private static int TrouverLigneEntetes(List<string> lignes)
    {
        for (var i = 0; i < lignes.Count; i++)
        {
            var champs = SplitCsv(lignes[i]);
            if (champs.Count >= 10 && Normaliser(champs[0]) == "compte")
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Associe chaque colonne attendue a son index reel, pour resister a un changement
    /// d'ordre ou a l'ajout de colonnes par la banque.
    /// </summary>
    private static Dictionary<string, int> MapperColonnes(List<string> entetes)
    {
        var map = new Dictionary<string, int>();

        for (var i = 0; i < entetes.Count; i++)
        {
            var nom = Normaliser(entetes[i]);

            if (nom.StartsWith("date de comptabilisation")) map.TryAdd("dateCompta", i);
            else if (nom.StartsWith("date valeur")) map.TryAdd("dateValeur", i);
            else if (nom.StartsWith("montant")) map.TryAdd("montant", i);
            else if (nom.StartsWith("transaction")) map.TryAdd("transaction", i);
            else if (nom.StartsWith("nom contrepartie")) map.TryAdd("nomContrepartie", i);
            else if (nom.StartsWith("compte contrepartie")) map.TryAdd("compteContrepartie", i);
            else if (nom.StartsWith("communication")) map.TryAdd("communication", i);
        }

        return map;
    }

    private static MouvementBancaire? ParserLigne(List<string> champs, Dictionary<string, int> colonnes)
    {
        var description = NettoyerEspaces(Champ(champs, colonnes, "transaction"));
        var montantBrut = Champ(champs, colonnes, "montant");

        if (string.IsNullOrWhiteSpace(montantBrut))
            return null;

        if (!decimal.TryParse(montantBrut, NumberStyles.Number, MontantFormat, out var montant))
            return null;

        var dateValeur = ParserDate(Champ(champs, colonnes, "dateValeur"));
        var dateCompta = ParserDate(Champ(champs, colonnes, "dateCompta"));

        // La date valeur est la reference ; a defaut on retombe sur la comptabilisation.
        var date = dateValeur ?? dateCompta;
        if (date is null)
            return null;

        var nomContrepartie = NettoyerEspaces(Champ(champs, colonnes, "nomContrepartie"));

        return new MouvementBancaire
        {
            DateValeur = date.Value,
            DateComptabilisation = dateCompta ?? date.Value,
            Montant = montant,
            Description = description,
            NomContrepartie = string.IsNullOrWhiteSpace(nomContrepartie) ? null : nomContrepartie,
            CompteContrepartie = NettoyerEspaces(Champ(champs, colonnes, "compteContrepartie")) is { Length: > 0 } iban
                ? iban
                : null,
            Communication = NettoyerEspaces(Champ(champs, colonnes, "communication")) is { Length: > 0 } com
                ? com
                : null,
            ExternalRef = ConstruireExternalRef(description, date.Value, montant),
            Type = DeduireType(description)
        };
    }

    private static string Champ(List<string> champs, Dictionary<string, int> colonnes, string cle)
        => colonnes.TryGetValue(cle, out var index) && index < champs.Count
            ? champs[index].Trim()
            : string.Empty;

    private static DateTime? ParserDate(string valeur)
        => DateTime.TryParseExact(valeur.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date)
            ? date
            : null;

    /* =======================
     * IDENTIFIANT ANTI-DOUBLON
     * ======================= */

    /// <summary>
    /// Utilise la reference bancaire du mouvement quand elle est presente (unique et stable).
    /// Sinon, retombe sur une empreinte de la date, du montant et du libelle.
    /// </summary>
    private static string ConstruireExternalRef(string description, DateTime date, decimal montant)
    {
        var matches = RefBancaireRegex().Matches(description);
        if (matches.Count > 0)
        {
            var reference = matches[^1].Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(reference))
                return Tronquer($"BEL:{reference}", 100);
        }

        var empreinte = $"{date:yyyyMMdd}|{montant.ToString(CultureInfo.InvariantCulture)}|{description}";
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(empreinte)));

        return $"BEL:H:{hash[..24]}";
    }

    /* =======================
     * CLASSIFICATION
     * ======================= */

    /// <summary>
    /// Deduit la nature du mouvement a partir du prefixe du libelle Belfius.
    /// L'ordre des tests compte : "REMBOURSEMENT PAIEMENT DEBITMASTERCARD" doit sortir
    /// avant "DEBITMASTERCARD", et "RETRAIT D'ESPECES" avant "BANCONTACT - ACHAT".
    /// </summary>
    private static MouvementBancaireType DeduireType(string description)
    {
        var texte = Normaliser(description).ToUpperInvariant();

        if (texte.Contains("REMBOURSEMENT")) return MouvementBancaireType.Remboursement;
        if (texte.Contains("RETRAIT D'ESPECES") || texte.Contains("RETRAIT DESPECES")) return MouvementBancaireType.Cash;
        if (texte.Contains("DOMICILIATION")) return MouvementBancaireType.Domiciliation;
        if (texte.Contains("ORDRE PERMANENT")) return MouvementBancaireType.OrdrePermanent;
        if (texte.Contains("CREDIT LOGEMENT") || texte.Contains("PAIEMENTS DUS POUR VOTRE CREDIT")) return MouvementBancaireType.Credit;
        if (texte.Contains("BANCONTACT") || texte.Contains("DEBITMASTERCARD") || texte.Contains("MASTERCARD")) return MouvementBancaireType.AchatCarte;
        if (texte.Contains("VERSEMENT")) return MouvementBancaireType.Versement;
        if (texte.Contains("VIREMENT")) return MouvementBancaireType.Virement;
        if (texte.Contains("PARTICIPATION AUX FRAIS") || texte.Contains("FRAIS DE GESTION")) return MouvementBancaireType.Frais;
        if (texte.Contains("INTERETS")) return MouvementBancaireType.Interet;

        return MouvementBancaireType.Autre;
    }

    /* =======================
     * OUTILS CSV
     * ======================= */

    /// <summary>
    /// Decoupe une ligne CSV sur le point-virgule, en respectant les champs entre guillemets.
    /// </summary>
    private static List<string> SplitCsv(string ligne)
    {
        var champs = new List<string>();
        var courant = new StringBuilder();
        var dansGuillemets = false;

        for (var i = 0; i < ligne.Length; i++)
        {
            var c = ligne[i];

            if (c == '"')
            {
                // Guillemet double a l'interieur d'un champ echappe : ""
                if (dansGuillemets && i + 1 < ligne.Length && ligne[i + 1] == '"')
                {
                    courant.Append('"');
                    i++;
                }
                else
                {
                    dansGuillemets = !dansGuillemets;
                }
            }
            else if (c == ';' && !dansGuillemets)
            {
                champs.Add(courant.ToString());
                courant.Clear();
            }
            else
            {
                courant.Append(c);
            }
        }

        champs.Add(courant.ToString());
        return champs;
    }

    /// <summary>Les libelles Belfius sont completes par des espaces de remplissage.</summary>
    private static string NettoyerEspaces(string valeur)
        => EspacesMultiplesRegex().Replace(valeur ?? string.Empty, " ").Trim();

    /// <summary>Minuscules sans accents, pour comparer entetes et libelles de facon robuste.</summary>
    private static string Normaliser(string valeur)
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

    private static string Tronquer(string valeur, int max)
        => valeur.Length <= max ? valeur : valeur[..max];

    [GeneratedRegex(@"REF\.\s*:\s*(\S+)\s+VAL\.")]
    private static partial Regex RefBancaireRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacesMultiplesRegex();
}
