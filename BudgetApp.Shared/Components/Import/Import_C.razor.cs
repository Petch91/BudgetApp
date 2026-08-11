using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Front_BudgetApp.Services.Notifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BudgetApp.Shared.Components.Import;

public partial class Import_C : ComponentBase
{
    [Inject] public IHttpImport HttpImport { get; set; } = default!;
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;
    [Inject] public IAppToastService ToastService { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    /// <summary>Onglet des transactions variables, d'ou l'on vient et ou l'on retourne.</summary>
    private const string PageTransactions = "/depenses#variables";

    /// <summary>Taille maximale acceptee cote client (alignee sur l'API).</summary>
    private const long TailleMaxFichier = 5 * 1024 * 1024;

    private List<CategorieDto> _categories = [];
    private List<ImportedTransactionDto> _lignes = [];
    private ImportPreviewDto? _preview;

    private string? _nomFichier;
    private string? _errorMessage;
    private bool _isAnalyzing;
    private bool _isImporting;
    private bool _initialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized)
        {
            _initialized = true;
            ToastService.ExecuteQueue();
            await ChargerCategories();
            StateHasChanged();
        }
    }

    private async Task ChargerCategories()
    {
        var result = await HttpCategorie.GetCategories();
        if (result.IsSuccess)
            _categories = result.Value.ToList();
    }

    /* =======================
     * ANALYSE
     * ======================= */

    private async Task AnalyserFichier(InputFileChangeEventArgs e)
    {
        var fichier = e.File;

        if (fichier.Size > TailleMaxFichier)
        {
            _errorMessage = "Fichier trop volumineux (5 Mo maximum)";
            return;
        }

        _errorMessage = null;
        _preview = null;
        _lignes = [];
        _nomFichier = fichier.Name;

        // Le fichier doit etre lu AVANT tout re-rendu : passer _isAnalyzing a true retire
        // l'element <InputFile> du DOM, ce qui invalide la reference JS du fichier cote
        // navigateur (erreur "_blazorFilesById"). On copie donc en memoire d'abord.
        MemoryStream buffer;

        try
        {
            await using var source = fichier.OpenReadStream(TailleMaxFichier);
            buffer = new MemoryStream();
            await source.CopyToAsync(buffer);
            buffer.Position = 0;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Erreur lors de la lecture du fichier : {ex.Message}";
            StateHasChanged();
            return;
        }

        _isAnalyzing = true;
        StateHasChanged();

        try
        {
            using (buffer)
            {
                var result = await HttpImport.Analyser(buffer, fichier.Name);

                if (result.IsFailed)
                {
                    _errorMessage = string.Join(" | ", result.Errors.Select(err => err.Message));
                    return;
                }

                _preview = result.Value;
                _lignes = result.Value.Lignes.ToList();

                if (_lignes.Count == 0)
                    _errorMessage = "Aucun mouvement trouve dans ce fichier";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Erreur lors de l'analyse : {ex.Message}";
        }
        finally
        {
            _isAnalyzing = false;
            StateHasChanged();
        }
    }

    /* =======================
     * EDITION DE L'APERCU
     * ======================= */

    private void BasculerLigne(int index)
    {
        if (index < 0 || index >= _lignes.Count) return;
        _lignes[index] = _lignes[index] with { Include = !_lignes[index].Include };
    }

    private void ChangerCategorie(int index, ChangeEventArgs e)
    {
        if (index < 0 || index >= _lignes.Count) return;
        if (!int.TryParse(e.Value?.ToString(), out var categorieId)) return;

        var nom = _categories.FirstOrDefault(c => c.Id == categorieId)?.Name ?? string.Empty;
        _lignes[index] = _lignes[index] with { CategorieId = categorieId, CategorieName = nom };
    }

    private void ToutSelectionner(bool selectionner)
    {
        for (var i = 0; i < _lignes.Count; i++)
        {
            // Un doublon reste exclu : il est deja en base.
            if (selectionner && _lignes[i].IsDuplicate) continue;
            _lignes[i] = _lignes[i] with { Include = selectionner };
        }
    }

    private int NbSelectionnees => _lignes.Count(l => l.Include);

    private decimal TotalDebitsSelection
        => _lignes.Where(l => l is { Include: true, IsRevenu: false }).Sum(l => l.Montant);

    private decimal TotalCreditsSelection
        => _lignes.Where(l => l is { Include: true, IsRevenu: true }).Sum(l => l.Montant);

    /* =======================
     * IMPORT
     * ======================= */

    private async Task ImporterSelection()
    {
        var selection = _lignes.Where(l => l.Include).ToList();

        if (selection.Count == 0)
        {
            ToastService.Info("Aucune ligne selectionnee", "Import");
            ToastService.ExecuteQueue();
            return;
        }

        _isImporting = true;
        StateHasChanged();

        var transactionsCreees = false;

        try
        {
            var form = new ImportConfirmForm
            {
                Lignes = selection.Select(l => new ImportLigneForm
                {
                    ExternalRef = l.ExternalRef,
                    Date = l.Date,
                    Intitule = l.Intitule,
                    Montant = l.Montant,
                    IsRevenu = l.IsRevenu,
                    CategorieId = l.CategorieId
                }).ToList()
            };

            var result = await HttpImport.Confirmer(form);

            if (result.IsFailed)
            {
                ToastService.Error(string.Join(" | ", result.Errors.Select(e => e.Message)), "Import");
                ToastService.ExecuteQueue();
                return;
            }
            
            var bilan = result.Value;
            var message = bilan.NbIgnorees > 0
                ? $"{bilan.NbCreees} transaction(s) importée(s), {bilan.NbIgnorees} doublon(s) ignoré(s)"
                : $"{bilan.NbCreees} transaction(s) importée(s)";

            // Le toast survit a la navigation : <Toasts> vit dans MainLayout et le service
            // est Scoped, donc lie au circuit et non a la page.
            ToastService.Success(message, "Import");
            ToastService.ExecuteQueue();

            transactionsCreees = bilan.NbCreees > 0;
            Reinitialiser();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Erreur lors de l'import : {ex.Message}", "Import");
            ToastService.ExecuteQueue();
        }
        finally
        {
            _isImporting = false;
            StateHasChanged();
        }

        // Retour a la liste pour constater le resultat. Si tout etait en doublon, rien n'a
        // change en base : on reste sur place, sinon l'utilisateur serait renvoye sans
        // comprendre pourquoi il ne voit aucune nouvelle ligne.
        // Hors du try/finally : plus rien a rafraichir une fois la navigation lancee.
        if (transactionsCreees)
            RetourTransactions();
    }

    private void RetourTransactions()
    {
        Navigation.NavigateTo(PageTransactions);
    }

    private void Reinitialiser()
    {
        _preview = null;
        _lignes = [];
        _nomFichier = null;
        _errorMessage = null;
    }

    /* =======================
     * AFFICHAGE
     * ======================= */

    private static string ClasseLigne(ImportedTransactionDto ligne)
    {
        if (ligne.IsDuplicate) return "ligne-doublon";
        if (!ligne.Include) return "ligne-exclue";
        return string.Empty;
    }
}
