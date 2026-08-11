using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using Front_BudgetApp.Services.Notifications;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace BudgetApp.Shared.Components.Transactions;

public partial class TransactionVariable_C : ComponentBase
{
    [Inject] public IHttpTransaction HttpTransaction { get; set; } = default!;
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;
    [Inject] public IAppToastService ToastService { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

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
    private string _searchTerm = string.Empty;
    private string _sortColumn = "Date";
    private bool _sortAscending = false;

    private IEnumerable<TransactionVariableDto> GetTransactionsFiltrees()
    {
        var filtered = string.IsNullOrWhiteSpace(_searchTerm)
            ? _transactions
            : _transactions.Where(t => t.Intitule.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));

        return _sortColumn switch
        {
            "Date"     => _sortAscending ? filtered.OrderBy(t => t.Date)     : filtered.OrderByDescending(t => t.Date),
            "Intitule" => _sortAscending ? filtered.OrderBy(t => t.Intitule)  : filtered.OrderByDescending(t => t.Intitule),
            "Montant"  => _sortAscending ? filtered.OrderBy(t => t.Montant)   : filtered.OrderByDescending(t => t.Montant),
            "Type"     => _sortAscending ? filtered.OrderBy(t => t.TransactionType) : filtered.OrderByDescending(t => t.TransactionType),
            _          => filtered.OrderByDescending(t => t.Date)
        };
    }

    private void TrierPar(string colonne)
    {
        if (_sortColumn == colonne)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = colonne;
            _sortAscending = colonne != "Date";
        }
    }

    private string GetSortIcon(string colonne)
    {
        if (_sortColumn != colonne) return "bi-chevron-expand";
        return _sortAscending ? "bi-chevron-up" : "bi-chevron-down";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ToastService.ExecuteQueue();
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
        else
        {
            Log.Warning("Échec chargement catégories : {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
            ToastService.Error(result.Errors.First().Message);
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
                Log.Warning("Échec chargement transactions {Month}/{Year} : {Error}", _selectedDate.Month, _selectedDate.Year, _errorMessage);
                ToastService.Error(_errorMessage);
            }
            else
            {
                _transactions = result.Value.ToList();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Erreur lors du chargement: {ex.Message}";
            Log.Error(ex, "Erreur inattendue chargement transactions");
            ToastService.Error(_errorMessage);
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
            : "Nouvelle dépense";
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

            var moisTransaction = new DateTime(_form.Date.Year, _form.Date.Month, 1);
            var moisDifferent = moisTransaction != new DateTime(_selectedDate.Year, _selectedDate.Month, 1);

            if (_transactionEnEdition is null)
            {
                var result = await HttpTransaction.Add(_form);
                if (result.IsFailed)
                {
                    var error = string.Join(" | ", result.Errors.Select(e => e.Message));
                    ToastService.Error(error);
                    return;
                }

                if (moisDifferent)
                {
                    ToastService.Info($"Transaction placée en {moisTransaction.ToString("MMMM yyyy")}");
                    _selectedDate = moisTransaction;
                    await FermerModal();
                    await LoadTransactionsAsync();
                }
                else
                {
                    _transactions.Add(result.Value);
                    await FermerModal();
                }

                ToastService.Success("Transaction ajoutée avec succès");
            }
            else
            {
                var result = await HttpTransaction.Update(_transactionEnEdition.Id, _form);
                if (result.IsFailed)
                {
                    var error = string.Join(" | ", result.Errors.Select(e => e.Message));
                    ToastService.Error(error);
                    return;
                }

                if (moisDifferent)
                {
                    ToastService.Info($"Transaction déplacée en {moisTransaction.ToString("MMMM yyyy")}");
                    _selectedDate = moisTransaction;
                    await FermerModal();
                    await LoadTransactionsAsync();
                }
                else
                {
                    var categorie = _categories.First(c => c.Id == _selectedCategorieId);
                    var updated = new TransactionVariableDto(
                        _transactionEnEdition.Id,
                        _form.Intitule,
                        _form.Montant,
                        _form.Date,
                        _form.TransactionType,
                        categorie);
                    var idx = _transactions.FindIndex(t => t.Id == _transactionEnEdition.Id);
                    if (idx >= 0) _transactions[idx] = updated;
                    await FermerModal();
                }

                ToastService.Success("Transaction mise à jour avec succès");
            }
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmerSuppression(TransactionVariableDto transaction)
    {
        var typeLabel = transaction.TransactionType == TransactionType.Revenu ? "revenu" : "dépense";

        var confirmation = await _confirmDialog.ShowAsync(
            title: "Confirmer la suppression",
            message1: $"Voulez-vous vraiment supprimer {typeLabel} \"{transaction.Intitule}\" ?",
            message2: "Cette action est irréversible.",
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
                var error = string.Join(" | ", result.Errors.Select(e => e.Message));
                ToastService.Error(error);
            }
            else
            {
                _transactions.Remove(transaction);
                ToastService.Success("Transaction supprimée avec succès");
            }
        }
    }

    private void GoImportPage()
    {
        Navigation.NavigateTo("/import");
    }
}
