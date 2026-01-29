using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using Front_BudgetApp.Services.Notifications;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Transactions;

public partial class DepenseFixe_C : ComponentBase
{
    [Inject] public IHttpDepenseFixe HttpDepense { get; set; } = default!;
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;
    [Inject] public IAppToastService ToastService { get; set; } = default!;

    private bool IsLoading = true;
    private bool _isSaving;
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

    private DateTime ObtenirProchainPaiement(DepenseFixeDto depense)
    {
        // Pour les dépenses échelonnées, calculer à partir de la date de début + échéances passées
        if (depense.IsEchelonne && depense.NombreEcheances.HasValue && depense.EcheancesRestantes.HasValue)
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
        if (depense.EstDomiciliee || depense.ReminderDaysBefore == 0) return false;

        var prochaineDate = ObtenirProchainPaiement(depense);
        var joursRestants = (prochaineDate - DateTime.Today).Days;
        return joursRestants <= depense.ReminderDaysBefore && joursRestants > 0;
    }

    private bool EstRappelUrgent(DepenseFixeDto depense)
    {
        if (depense.EstDomiciliee) return false;

        var prochaineDate = ObtenirProchainPaiement(depense);
        var joursRestants = (prochaineDate - DateTime.Today).Days;
        return joursRestants <= 3;
    }

    private List<DepenseFixeDto> ObtenirDepensesAvecRappelUrgent()
        => _depenses.Where(d => d.IsActive && EstRappelUrgent(d)).ToList();

    private int CompterRappelsActifs()
        => _depenses.Count(d => d.IsActive && (EstRappelActif(d) || EstRappelUrgent(d)));

    private decimal CalculerTotalMensuel()
    {
        var dateMoisProchain = DateTime.Today.AddMonths(1);
        return _depenses
            .Where(d => d.IsActive)
            .Where(d =>
            {
                var prochainPaiement = ObtenirProchainPaiement(d);
                return prochainPaiement.Month == dateMoisProchain.Month
                       && prochainPaiement.Year == dateMoisProchain.Year;
            })
            .Sum(d => d.Montant);
    }

    private int CompterDepensesActives()
        => _depenses.Count(d => d.IsActive);

    private int CompterDepensesTerminees()
        => _depenses.Count(d => !d.IsActive);

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
        if (estUrgent)
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
        var rappelNonVu = depense.Rappels
            .Where(r => !r.Vu && r.RappelDate <= DateTime.Today)
            .OrderBy(r => r.RappelDate)
            .FirstOrDefault();

        if (rappelNonVu is not null)
        {
            var result = await HttpDepense.ChangeVuRappel(rappelNonVu.Id);
            if (result.IsSuccess)
            {
                ToastService.Info("Rappel marque comme lu", "Rappel");
                ToastService.ExecuteQueue();
                await LoadAsync();
            }
        }
    }
}
