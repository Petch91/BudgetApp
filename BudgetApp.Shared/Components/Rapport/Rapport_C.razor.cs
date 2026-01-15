using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Rapport;

public partial class Rapport_C : ComponentBase
{
    [Inject] private IHttpRapport HttpRapport { get; set; } = null!;

    private DateTime _selectedDate = DateTime.Today;
    private RapportMoisDto? _rapport;
    private bool _isLoading;
    private string? _errorMessage;
    private int? _selectedCategorieId;

    protected override async Task OnInitializedAsync()
    {
        await ChargerRapport();
    }

    private async Task ChargerRapport()
    {
        _isLoading = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await HttpRapport.GetRapportMois(_selectedDate.Year, _selectedDate.Month);

            if (result.IsSuccess)
            {
                _rapport = result.Value;
            }
            else
            {
                _errorMessage = "Impossible de charger le rapport";
            }
        }
        catch (Exception)
        {
            _errorMessage = "Erreur technique lors du chargement";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task MoisPrecedent()
    {
        _selectedDate = _selectedDate.AddMonths(-1);
        _selectedCategorieId = null;
        await ChargerRapport();
    }

    private async Task MoisSuivant()
    {
        _selectedDate = _selectedDate.AddMonths(1);
        _selectedCategorieId = null;
        await ChargerRapport();
    }

    private void FiltrerParCategorie(int? categorieId)
    {
        _selectedCategorieId = categorieId;
    }

    private IEnumerable<CategorieDto> GetCategoriesFromRapport()
    {
        if (_rapport is null) return [];

        return _rapport.Lignes
            .Select(l => l.Categorie)
            .DistinctBy(c => c.Id)
            .OrderBy(c => c.Name);
    }

    private IEnumerable<RapportLigneDto> GetLignesFiltrees()
    {
        if (_rapport is null) return [];

        if (_selectedCategorieId is null)
            return _rapport.Lignes;

        return _rapport.Lignes.Where(l => l.Categorie.Id == _selectedCategorieId);
    }

    /* =======================
     * HELPERS AFFICHAGE SOLDE
     * ======================= */

    private string GetSoldeCardClass()
    {
        if (_rapport is null) return "h-100 border-0 shadow-sm";

        var borderColor = _rapport.Solde >= 0 ? "border-success" : "border-danger";
        return $"h-100 border-0 shadow-sm border-start {borderColor} border-4";
    }

    private string GetSoldeIconBgClass()
    {
        if (_rapport is null) return "bg-secondary bg-opacity-10";
        return _rapport.Solde >= 0 ? "bg-success bg-opacity-10" : "bg-danger bg-opacity-10";
    }
    private string GetligneBgClass(RapportLigneDto ligne)
    {
        if (ligne is null) return "";
        return ligne.IsRevenu ? "table-success" : "table-danger";
    }

    private IconColor GetSoldeIconColor()
    {
        if (_rapport is null) return IconColor.Secondary;
        return _rapport.Solde >= 0 ? IconColor.Success : IconColor.Danger;
    }

    private string GetSoldeTextClass()
    {
        if (_rapport is null) return "";
        return _rapport.Solde >= 0 ? "text-success" : "text-danger";
    }

    private string GetSoldeDisplay()
    {
        if (_rapport is null) return "0,00 \u20ac";

        var prefix = _rapport.Solde >= 0 ? "+" : "";
        return $"{prefix}{_rapport.Solde:N2} \u20ac";
    }
}
