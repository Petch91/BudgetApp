using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Domain.Models;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Transactions;

public partial class DepenseFixe_C : ComponentBase
{
    [Inject] public IHttpDepenseFixe HttpDepense { get; set; } = default!;

    private bool IsLoading = true;
    private bool _gridInitialized;
    private List<DepenseFixeDto> _depenses = [];
    private string? errorMessage;

    private Grid<DepenseFixeDto> grid = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadAsync();
            return;
        }

        // ⚠️ refresh grid UNIQUEMENT après chargement
        if (!_gridInitialized && !IsLoading && grid is not null)
        {
            _gridInitialized = true;
            await grid.RefreshDataAsync();
        }
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        StateHasChanged(); // affiche spinner

        var result = await HttpDepense.GetDepenses();

        if (result.IsFailed)
        {
            errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
            IsLoading = false;
            StateHasChanged();
            return;
        }

        _depenses = result.Value.ToList();

        IsLoading = false;
        StateHasChanged(); // cache spinner → affiche grid
    }

    private string GetRowStyle(DepenseFixeDto item)
        => item.EstDomiciliee ? "background-color: #d4edda;" : "";
    
    private DateTime ObtenirProchainPaiement(DepenseFixeDto depense)
    {
        var prochaineDueDate = depense.DueDates
            .Where(d => d.date >= DateTime.Today)
            .OrderBy(d => d.date)
            .FirstOrDefault();

        return prochaineDueDate?.date ?? depense.DueDates.Select(d => d.date).Max();
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
    {
        return _depenses.Where(d => EstRappelUrgent(d)).ToList();
    }
    
    private int CompterRappelsActifs()
    {
        return _depenses.Count(d => EstRappelActif(d) || EstRappelUrgent(d));
    }

    private decimal CalculerTotalMensuel()
    {
        var date = DateTime.UtcNow.AddMonths(1);
        var filteredDepense = _depenses.Where(d => ObtenirProchainPaiement(d).Month == date.Month && ObtenirProchainPaiement(d).Year == date.Year);
        var sum = filteredDepense.Sum(d => d.Montant);
        return sum;
    }
    
    private void OnAjouterDepense()
    {
        // TODO: Naviguer vers formulaire d'ajout
    }
}