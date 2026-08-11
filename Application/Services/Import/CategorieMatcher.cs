using System.Globalization;
using System.Text;
using Entities.Domain.Models;

namespace Application.Services.Import;

/// <summary>
/// Suggere une categorie a partir du marchand ou du libelle d'un mouvement bancaire.
///
/// Ce n'est qu'une suggestion : l'utilisateur corrige dans l'ecran de revue avant import.
/// On prefere donc rater une categorie (et retomber sur la categorie par defaut) plutot
/// que d'en imposer une fausse.
/// </summary>
public static class CategorieMatcher
{
    /// <summary>
    /// Regles marchand -> nom de categorie. L'ordre compte : la premiere regle qui matche gagne.
    /// Les noms de categorie doivent correspondre a ceux presents en base ; s'ils n'existent
    /// pas, le service retombe sur la categorie par defaut.
    /// </summary>
    private static readonly (string Categorie, string[] MotsCles)[] Regles =
    [
        // Employeurs et organismes payeurs : noms tres distinctifs, teste en premier.
        ("Travail", ["AUTOMATION-TECHNIQUE", "FEDERAAL AGENTSCHAP", "ONEM", "SALAIRE", "PECULE"]),

        ("Alimentaire", ["LIDL", "DELHAIZE", "INTERMARCHE", "CARREFOUR", "CRF MKT", "ALDI", "COLRUYT",
                         "SPAR", "MILLE SAVEURS", "BOULANGERIE", "PATISSERIE", "GRAINETERIE",
                         "ATELIER DE MATHIEU", "PROXY", "OKAY"]),

        ("Travaux", ["HUBO", "BRICO", "GAMMA", "LEROY MERLIN", "BRICOLAGE", "MAKRO"]),

        ("Transport", ["CARBURANT", "Q8", "TOTALENERGIES", "ESSO", "SHELL", "LUKOIL", "TEXACO",
                       "AUTOROUTE", "PARKING", "SNCB", "NMBS", "TEC", "DE LIJN", "STIB"]),

        ("Medical", ["PHARMACIE", "FARMALINE", "MOLITOR", "MEDECIN", "DENTISTE",
                     "HOPITAL", "CLINIQUE", "KINE", "OPTIQUE"]),

        ("Animaux", ["VETERINAIRE", "TOM&CO", "TOM & CO", "ANIMALERIE", "MAXI ZOO"]),

        ("Restaurant", ["BURGER KING", "MCDONALD", "QUICK", "RESTAURANT", "PIZZA", "PIZZERIA",
                        "FRITERIE", "TAVERNE", "BRASSERIE", "PASSERELLE", "TUILERIES",
                        "LA VIE EN VERT", "SNACK", "TAKEAWAY", "DELIVEROO"]),

        ("Telecom", ["ORANGE", "PROXIMUS", "TELENET", "SCARLET", "VOO", "MOBILE VIKINGS", "BASE"]),

        ("Energie", ["ENECO", "ENGIE", "LUMINUS", "LAMPIRIS", "MEGA", "OCTA+", "SWDE",
                     "SOCIETE WALLONNE DES EAUX", "RESA", "ORES"]),

        ("Assurances", ["SOLIDARIS", "ASSURANCE", "AG INSURANCE", "AXA", "ETHIAS", "P&V",
                        "MUTUALITE", "PARTENAMUT", "HELAN", "DKV"]),

        ("Logement", ["CREDIT LOGEMENT", "LOYER", "HOME CREDIT", "SYNDIC"]),

        ("Impots", ["SERVICE PUBLIC DE WALLONIE", "SPF FINANCES", "CONTRIBUTIONS", "CADASTRE",
                    "PRECOMPTE", "TAXE"]),

        ("Loisirs", ["PHANTASIALAND", "UBER", "NETFLIX", "SPOTIFY", "DISNEY", "CINEMA", "KINEPOLIS",
                     "APPLE.COM", "PLAYSTATION", "STEAM", "NINTENDO", "DECATHLON"]),

        ("Shopping", ["AMZN", "AMAZON", "ACTION", "ZALANDO", "VINTED", "MEDIA MARKT", "KRUIDVAT",
                      "ROWENTA", "ALMA SAS", "COOLBLUE", "BOL.COM", "IKEA", "TEMU", "SHEIN"]),

        ("Vetements", ["C&A", "H&M", "ZARA", "PRIMARK", "JBC", "BEL&BO", "ESPRIT"]),

        ("Banques", ["PARTICIPATION AUX FRAIS", "FRAIS DE GESTION", "INTERETS", "COTISATION CARTE"])
    ];

    /// <summary>
    /// Renvoie le nom de categorie suggere, ou null si aucune regle ne correspond.
    /// Le marchand est teste en premier : c'est le signal le plus fiable.
    /// </summary>
    public static string? Suggerer(MouvementBancaire mouvement)
    {
        var marchand = Normaliser(mouvement.NomContrepartie);
        if (!string.IsNullOrEmpty(marchand))
        {
            var parMarchand = Chercher(marchand);
            if (parMarchand is not null)
                return parMarchand;
        }

        return Chercher(Normaliser(mouvement.Description));
    }

    /// <summary>
    /// Longueur a partir de laquelle un mot-cle peut matcher un prefixe de mot
    /// ("ASSURANCE" doit trouver "ASSURANCES", "PHARMACIE" doit trouver "PHARMACIEMOLITOR").
    /// En dessous, on exige le mot entier, sans quoi des sigles comme "TEC" matcheraient
    /// n'importe quel mot les contenant ("TECHNIQUE").
    /// </summary>
    private const int LongueurPrefixeAutorise = 6;

    private static string? Chercher(string texte)
    {
        if (string.IsNullOrEmpty(texte))
            return null;

        foreach (var (categorie, motsCles) in Regles)
        {
            foreach (var motCle in motsCles)
            {
                if (Contient(texte, Normaliser(motCle)))
                    return categorie;
            }
        }

        return null;
    }

    /// <summary>
    /// Cherche le mot-cle en exigeant qu'il commence sur une frontiere de mot.
    /// Un mot-cle court doit correspondre au mot entier ; un mot-cle long peut n'en
    /// couvrir que le debut, pour absorber pluriels et libelles colles.
    /// </summary>
    private static bool Contient(string texte, string motCle)
    {
        if (string.IsNullOrEmpty(motCle))
            return false;

        var index = 0;

        while ((index = texte.IndexOf(motCle, index, StringComparison.Ordinal)) >= 0)
        {
            var debutDeMot = index == 0 || !char.IsLetterOrDigit(texte[index - 1]);

            if (debutDeMot)
            {
                var apres = index + motCle.Length;
                var finDeMot = apres >= texte.Length || !char.IsLetterOrDigit(texte[apres]);

                if (finDeMot || motCle.Length >= LongueurPrefixeAutorise)
                    return true;
            }

            index++;
        }

        return false;
    }

    /// <summary>Majuscules sans accents, pour comparer marchands et libelles de facon robuste.</summary>
    private static string Normaliser(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
            return string.Empty;

        var decompose = valeur.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decompose.Length);

        foreach (var c in decompose)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
