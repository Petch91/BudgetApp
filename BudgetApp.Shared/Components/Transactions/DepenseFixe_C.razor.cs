using System.Globalization;
using System.Text;
using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using Front_BudgetApp.Services.Notifications;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BudgetApp.Shared.Components.Transactions;

public partial class DepenseFixe_C : ComponentBase
{
    [Inject] public IHttpDepenseFixe HttpDepense { get; set; } = default!;
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;
    [Inject] public IHttpRapport HttpRapport { get; set; } = default!;
    [Inject] public IAppToastService ToastService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    private bool IsLoading = true;
    private bool _isSaving;
    private bool _isExporting;
    private string _exportMonth = DateTime.Today.ToString("yyyy-MM");
    private string? _errorMessage;

    private List<DepenseFixeDto> _depenses = [];
    private List<CategorieDto> _categories = [];

    private Modal _modalForm = default!;
    private ConfirmDialog _confirmDialog = default!;

    private DepenseFixeDto? _depenseEnEdition;
    private DepenseFixeForm _form = new();
    private int _selectedCategorieId;
    private Frequence _selectedFrequence = Frequence.Mensuel;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ToastService.ExecuteQueue();
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var depensesTask = HttpDepense.GetDepenses();
            var categoriesTask = HttpCategorie.GetCategories();

            await Task.WhenAll(depensesTask, categoriesTask);

            var depensesResult = await depensesTask;
            var categoriesResult = await categoriesTask;

            if (depensesResult.IsFailed)
            {
                _errorMessage = string.Join(" | ", depensesResult.Errors.Select(e => e.Message));
            }
            else
            {
                _depenses = depensesResult.Value.ToList();
            }

            if (categoriesResult.IsSuccess)
            {
                _categories = categoriesResult.Value.ToList();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Erreur lors du chargement: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /* =======================
     * EXPORT CSV
     * ======================= */

    private async Task ExporterCsv()
    {
        if (!DateTime.TryParseExact(_exportMonth, "yyyy-MM", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var mois))
        {
            ToastService.Error("Mois invalide", "Export");
            ToastService.ExecuteQueue();
            return;
        }

        _isExporting = true;
        StateHasChanged();

        try
        {
            var result = await HttpRapport.GetDepensesFixesMois(mois.Year, mois.Month);

            if (result.IsFailed)
            {
                ToastService.Error(string.Join(" | ", result.Errors.Select(e => e.Message)), "Export");
                ToastService.ExecuteQueue();
                return;
            }

            var lignes = result.Value;
            if (lignes.Count == 0)
            {
                ToastService.Info("Aucune dépense fixe pour ce mois", "Export");
                ToastService.ExecuteQueue();
                return;
            }

            var csv = ConstruireCsv(lignes);
            var filename = $"depenses-fixes-{_exportMonth}.csv";
            await JS.InvokeVoidAsync("budgetApp.downloadFile", filename, csv, "text/csv;charset=utf-8;");

            ToastService.Success($"{lignes.Count} dépense(s) fixe(s) exportée(s)", "Export");
            ToastService.ExecuteQueue();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Erreur lors de l'export : {ex.Message}", "Export");
            ToastService.ExecuteQueue();
        }
        finally
        {
            _isExporting = false;
            StateHasChanged();
        }
    }

    private static string ConstruireCsv(IReadOnlyList<DepenseFixeMoisDto> lignes)
    {
        var sb = new StringBuilder();
        sb.Append("Date;Intitulé;Catégorie;Fréquence;Échéance;Montant\r\n");

        foreach (var l in lignes)
        {
            var echeance = l is { NumeroEcheance: { } n, TotalEcheances: { } t }
                ? $"{n}/{t}"
                : string.Empty;
            var montant = l.Montant.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

            sb.Append(l.Date.ToString("dd/MM/yyyy")).Append(';')
                .Append(EchapperCsv(l.Intitule)).Append(';')
                .Append(EchapperCsv(l.Categorie)).Append(';')
                .Append(EchapperCsv(l.Frequence)).Append(';')
                .Append(echeance).Append(';')
                .Append(montant).Append("\r\n");
        }

        return sb.ToString();
    }

    private static string EchapperCsv(string? field)
    {
        field ??= string.Empty;
        if (field.Contains('"') || field.Contains(';') || field.Contains('\n') || field.Contains('\r'))
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        return field;
    }

    private decimal ObtenirMontantEcheance(DepenseFixeDto depense)
    {
        if (!depense.IsEchelonne || !depense.MontantParEcheance.HasValue || !depense.NombreEcheances.HasValue)
            return depense.Montant;

        if (depense.EcheancesRestantes == 1)
            return depense.Montant - (depense.NombreEcheances.Value - 1) * depense.MontantParEcheance.Value;

        return depense.MontantParEcheance.Value;
    }

    private static bool EchelonnementTermine(DepenseFixeDto depense)
        => depense.IsEchelonne && depense.EcheancesRestantes is <= 0;

    private DateTime ObtenirProchainPaiement(DepenseFixeDto depense)
    {
        // Pour les dépenses échelonnées encore en cours, calculer à partir de la date de début + échéances passées.
        // Échelonnement terminé (EcheancesRestantes == 0) : pas de prochaine échéance échelonnée, on retombe sur les DueDates.
        if (depense.IsEchelonne && depense.NombreEcheances.HasValue && depense.EcheancesRestantes > 0)
        {
            var startDate = depense.DueDates.Select(d => d.Date).Min();
            var numero = depense.NombreEcheances.Value - depense.EcheancesRestantes.Value;
            return startDate.AddMonths(numero);
        }

        var prochaineDueDate = depense.DueDates
            .Where(d => d.Date >= DateTime.Today)
            .OrderBy(d => d.Date)
            .FirstOrDefault();

        return prochaineDueDate?.Date ?? depense.DueDates.Select(d => d.Date).Max();
    }

    private bool EstRappelActif(DepenseFixeDto depense)
    {
        if (depense.EstDomiciliee || depense.ReminderDaysBefore == 0 || EchelonnementTermine(depense)) return false;

        var prochaineDate = ObtenirProchainPaiement(depense);
        var joursRestants = (prochaineDate - DateTime.Today).Days;
        return joursRestants <= depense.ReminderDaysBefore && joursRestants > 0;
    }

    private bool EstRappelUrgent(DepenseFixeDto depense)
    {
        if (depense.EstDomiciliee || EchelonnementTermine(depense)) return false;

        var prochaineDate = ObtenirProchainPaiement(depense);
        var joursRestants = (prochaineDate - DateTime.Today).Days;
        return joursRestants <= 3;
    }

    private List<DepenseFixeDto> ObtenirDepensesAvecRappelUrgent()
        => _depenses.Where(d => d.IsActive && EstRappelUrgent(d)).ToList();

    private int CompterRappelsActifs()
        => _depenses.Count(d => d.IsActive && (EstRappelActif(d) || EstRappelUrgent(d)));

    private decimal CalculerTotalMoisEnCours()
    {
        var aujourdhui = DateTime.Today;
        return _depenses
            .Where(d => d.IsActive)
            .Where(d =>
            {
                var prochainPaiement = ObtenirProchainPaiement(d);
                return prochainPaiement.Month == aujourdhui.Month
                       && prochainPaiement.Year == aujourdhui.Year
                       && prochainPaiement >= aujourdhui;
            })
            .Sum(d => ObtenirMontantEcheance(d));
    }

    private decimal CalculerMoyenneMensuelle()
    {
        return _depenses
            .Where(d => d.IsActive)
            .Sum(d =>
            {
                var montant = ObtenirMontantEcheance(d);
                return d.Frequence switch
                {
                    Frequence.Mensuel => montant,
                    Frequence.Trimestriel => montant / 3m,
                    Frequence.Biannuel => montant / 6m,
                    Frequence.Annuel => montant / 12m,
                    _ => montant
                };
            });
    }

    private decimal CalculerTotalAnnuel()
    {
        return _depenses
            .Where(d => d.IsActive)
            .Sum(d =>
            {
                var montant = ObtenirMontantEcheance(d);
                return d.Frequence switch
                {
                    Frequence.Mensuel => montant * 12m,
                    Frequence.Trimestriel => montant * 4m,
                    Frequence.Biannuel => montant * 2m,
                    Frequence.Annuel => montant,
                    _ => montant
                };
            });
    }

    private (int count, decimal total) CalculerEchelonnesRestants()
    {
        var echelonnes = _depenses
            .Where(d => d.IsActive && d.IsEchelonne && d.EcheancesRestantes > 0)
            .ToList();

        var count = echelonnes.Sum(d => d.EcheancesRestantes ?? 0);
        var total = echelonnes.Sum(d => (d.EcheancesRestantes ?? 0) * ObtenirMontantEcheance(d));

        return (count, total);
    }

    private (string intitule, DateTime date, decimal montant)? ObtenirProchaineEcheanceInfo()
    {
        var prochaine = _depenses
            .Where(d => d.IsActive)
            .Select(d => new { Depense = d, Date = ObtenirProchainPaiement(d) })
            .Where(x => x.Date >= DateTime.Today)
            .OrderBy(x => x.Date)
            .FirstOrDefault();

        if (prochaine is null)
            return null;

        return (prochaine.Depense.Intitule, prochaine.Date, ObtenirMontantEcheance(prochaine.Depense));
    }

    private static string GetFrequenceLabel(Frequence frequence) => frequence switch
    {
        Frequence.Mensuel => "Mensuel",
        Frequence.Trimestriel => "Trimestriel",
        Frequence.Biannuel => "Biannuel",
        Frequence.Annuel => "Annuel",
        _ => frequence.ToString()
    };

    private static BadgeColor GetFrequenceBadgeColor(Frequence frequence) => frequence switch
    {
        Frequence.Mensuel => BadgeColor.Primary,
        Frequence.Trimestriel => BadgeColor.Info,
        Frequence.Biannuel => BadgeColor.Warning,
        Frequence.Annuel => BadgeColor.Success,
        _ => BadgeColor.Secondary
    };

    private static IEnumerable<Frequence> GetAvailableFrequences()
        => new[] { Frequence.Mensuel, Frequence.Trimestriel, Frequence.Biannuel, Frequence.Annuel };

    private void SelectFrequence(Frequence frequence)
    {
        _selectedFrequence = frequence;
    }

    private List<DepenseFixeDto> GetDepensesForSelectedFrequence()
        => _depenses.Where(d => d.Frequence == _selectedFrequence).OrderBy(d => ObtenirProchainPaiement(d)).ToList();

    private static string GetRowClass(DepenseFixeDto depense, bool rappelNonVu, bool estUrgent)
    {
        // Priorité: inactif > rappel non vu > urgent
        if (!depense.IsActive)
            return "table-secondary opacity-75";
        if (rappelNonVu)
            return "row-rappel";
        if (estUrgent && rappelNonVu)
            return "table-warning";
        return "";
    }

    private async Task OuvrirModal(DepenseFixeDto? depense)
    {
        _depenseEnEdition = depense;

        if (depense is null)
        {
            _form = new DepenseFixeForm
            {
                BeginDate = DateTime.Today,
                Frequence = Frequence.Mensuel,
                ReminderDaysBefore = 3
            };
            _selectedCategorieId = _categories.FirstOrDefault()?.Id ?? 0;
        }
        else
        {
            _form = new DepenseFixeForm
            {
                Intitule = depense.Intitule,
                Montant = depense.Montant,
                Frequence = depense.Frequence,
                EstDomiciliee = depense.EstDomiciliee,
                ReminderDaysBefore = depense.ReminderDaysBefore,
                BeginDate = depense.DueDates.Select(d => d.Date).Min(),
                Categorie = depense.Categorie,
                DateFin = depense.DateFin,
                IsEchelonne = depense.IsEchelonne,
                NombreEcheances = depense.NombreEcheances,
                MontantParEcheance = depense.MontantParEcheance
            };
            _selectedCategorieId = depense.Categorie.Id;
        }

        await _modalForm.ShowAsync();
    }

    private async Task FermerModal()
    {
        await _modalForm.HideAsync();
        _depenseEnEdition = null;
    }

    private async Task SauvegarderDepense()
    {
        if (string.IsNullOrWhiteSpace(_form.Intitule) || _form.Montant <= 0 || _selectedCategorieId == 0)
        {
            return;
        }

        _isSaving = true;
        StateHasChanged();

        try
        {
            _form.Categorie = _categories.First(c => c.Id == _selectedCategorieId);

            if (_depenseEnEdition is null)
            {
                var result = await HttpDepense.Add(_form);
                if (result.IsFailed)
                {
                    _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
                    ToastService.Error("Erreur lors de l'ajout de la depense", "Ajout");
                    ToastService.ExecuteQueue();
                    return;
                }

                ToastService.Success($"Depense \"{_form.Intitule}\" ajoutee", "Ajout");
            }
            else
            {
                var result = await HttpDepense.Update(_depenseEnEdition.Id, _form);
                if (result.IsFailed)
                {
                    _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
                    ToastService.Error("Erreur lors de la modification", "Modification");
                    ToastService.ExecuteQueue();
                    return;
                }

                ToastService.Success($"Depense \"{_form.Intitule}\" modifiee", "Modification");
            }

            ToastService.ExecuteQueue();
            await FermerModal();
            await LoadAsync();
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmerSuppression(DepenseFixeDto depense)
    {
        var confirmation = await _confirmDialog.ShowAsync(
            title: "Confirmer la suppression",
            message1: $"Voulez-vous vraiment supprimer la depense \"{depense.Intitule}\" ?",
            message2: "Cette action est irreversible.",
            confirmDialogOptions: new ConfirmDialogOptions
            {
                YesButtonText = "Supprimer",
                YesButtonColor = ButtonColor.Danger,
                NoButtonText = "Annuler",
                NoButtonColor = ButtonColor.Secondary
            });

        if (confirmation)
        {
            var result = await HttpDepense.Delete(depense.Id);
            if (result.IsFailed)
            {
                _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
                ToastService.Error("Erreur lors de la suppression", "Suppression");
                ToastService.ExecuteQueue();
            }
            else
            {
                ToastService.Success($"Depense \"{depense.Intitule}\" supprimee", "Suppression");
                ToastService.ExecuteQueue();
                await LoadAsync();
            }
        }
    }

    private async Task MarquerRappelVu(DepenseFixeDto depense)
    {
        var rappelsNonVus = depense.Rappels
            .Where(r => !r.Vu && r.RappelDate <= DateTime.Today)
            .ToList();

        if (rappelsNonVus.Count == 0)
            return;

        foreach (var rappel in rappelsNonVus)
        {
            await HttpDepense.ChangeVuRappel(rappel.Id);
        }

        ToastService.Info($"{rappelsNonVus.Count} rappel(s) marque(s) comme lu(s)", "Rappel");
        ToastService.ExecuteQueue();
        await LoadAsync();
    }
}
