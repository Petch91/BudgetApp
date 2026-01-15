using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Transactions;

public partial class TransactionVariable_C : ComponentBase
{
    [Inject] public IHttpTransaction HttpTransaction { get; set; } = default!;
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;

    private bool IsLoading = true;
    private bool _isSaving;
    private string? _errorMessage;

    private DateTime _selectedDate = DateTime.Today;
    private List<TransactionVariableDto> _transactions = [];
    private List<CategorieDto> _categories = [];

    private Modal _modalForm = default!;
    private ConfirmDialog _confirmDialog = default!;

    private TransactionVariableDto? _transactionEnEdition;
    private TransactionVariableForm _form = new();
    private int _selectedCategorieId;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadCategoriesAsync();
            await LoadTransactionsAsync();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await HttpCategorie.GetCategories();
        if (result.IsSuccess)
        {
            _categories = result.Value.ToList();
        }
    }

    private async Task LoadTransactionsAsync()
    {
        IsLoading = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await HttpTransaction.GetByMonth(_selectedDate.Month, _selectedDate.Year);

            if (result.IsFailed)
            {
                _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
            }
            else
            {
                _transactions = result.Value.ToList();
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

    private async Task MoisPrecedent()
    {
        _selectedDate = _selectedDate.AddMonths(-1);
        await LoadTransactionsAsync();
    }

    private async Task MoisSuivant()
    {
        _selectedDate = _selectedDate.AddMonths(1);
        await LoadTransactionsAsync();
    }

    private decimal CalculerTotalRevenus()
        => _transactions
            .Where(t => t.TransactionType == TransactionType.Revenu)
            .Sum(t => t.Montant);

    private decimal CalculerTotalDepenses()
        => _transactions
            .Where(t => t.TransactionType == TransactionType.Depense)
            .Sum(t => t.Montant);

    private decimal CalculerSolde()
        => CalculerTotalRevenus() - CalculerTotalDepenses();

    private string GetSoldeCardClass()
    {
        var color = CalculerSolde() >= 0 ? "success" : "danger";
        return $"h-100 border-0 shadow-sm border-start border-{color} border-4";
    }

    private string GetSoldeIconBgClass()
    {
        var color = CalculerSolde() >= 0 ? "success" : "danger";
        return $"bg-{color} bg-opacity-10";
    }

    private IconColor GetSoldeIconColor()
        => CalculerSolde() >= 0 ? IconColor.Success : IconColor.Danger;

    private string GetSoldeTextClass()
    {
        var color = CalculerSolde() >= 0 ? "success" : "danger";
        return $"text-{color}";
    }

    private string GetSoldeDisplay()
    {
        var solde = CalculerSolde();
        var sign = solde >= 0 ? "+" : "";
        return $"{sign}{solde:N2} \u20ac";
    }

    private string GetModalTitle()
    {
        if (_transactionEnEdition is not null)
            return "Modifier la transaction";

        return _form.TransactionType == TransactionType.Revenu
            ? "Nouveau revenu"
            : "Nouvelle depense";
    }

    private async Task OuvrirModal(TransactionVariableDto? transaction, TransactionType type)
    {
        _transactionEnEdition = transaction;

        if (transaction is null)
        {
            _form = new TransactionVariableForm
            {
                Date = DateTime.Today,
                TransactionType = type
            };
            _selectedCategorieId = _categories.FirstOrDefault()?.Id ?? 0;
        }
        else
        {
            _form = new TransactionVariableForm
            {
                Intitule = transaction.Intitule,
                Montant = transaction.Montant,
                Date = transaction.Date,
                TransactionType = transaction.TransactionType,
                CategorieId = transaction.Categorie.Id
            };
            _selectedCategorieId = transaction.Categorie.Id;
        }

        await _modalForm.ShowAsync();
    }

    private async Task FermerModal()
    {
        await _modalForm.HideAsync();
        _transactionEnEdition = null;
    }

    private async Task SauvegarderTransaction()
    {
        if (string.IsNullOrWhiteSpace(_form.Intitule) || _form.Montant <= 0 || _selectedCategorieId == 0)
        {
            return;
        }

        _isSaving = true;
        StateHasChanged();

        try
        {
            _form.CategorieId = _selectedCategorieId;

            if (_transactionEnEdition is null)
            {
                var result = await HttpTransaction.Add(_form);
                if (result.IsFailed)
                {
                    _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
                    return;
                }
            }
            else
            {
                var result = await HttpTransaction.Update(_transactionEnEdition.Id, _form);
                if (result.IsFailed)
                {
                    _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
                    return;
                }
            }

            await FermerModal();

            // Recharger le mois de la transaction ajoutee/modifiee
            _selectedDate = new DateTime(_form.Date.Year, _form.Date.Month, 1);
            await LoadTransactionsAsync();
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmerSuppression(TransactionVariableDto transaction)
    {
        var typeLabel = transaction.TransactionType == TransactionType.Revenu ? "revenu" : "depense";

        var confirmation = await _confirmDialog.ShowAsync(
            title: "Confirmer la suppression",
            message1: $"Voulez-vous vraiment supprimer {typeLabel} \"{transaction.Intitule}\" ?",
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
            var result = await HttpTransaction.Delete(transaction.Id);
            if (result.IsFailed)
            {
                _errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
            }
            else
            {
                await LoadTransactionsAsync();
            }
        }
    }
}
