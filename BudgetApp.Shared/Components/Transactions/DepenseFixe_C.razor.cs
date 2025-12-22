using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Transactions;

public partial class DepenseFixe_C : ComponentBase
{
    [Inject] public IHttpDepenseFixe HttpDepense { get; set; } = default!;

    private List<DepenseFixeDto> _depenses = [];
    private bool loading = true;
    private bool _gridInitialized;
    private string? errorMessage;

    private Grid<DepenseFixeDto> grid = default!;

    protected override async Task OnInitializedAsync()
    {
        var result = await HttpDepense.GetDepenses();

        if (result.IsFailed)
        {
            errorMessage = string.Join(" | ", result.Errors.Select(e => e.Message));
            loading = false;
            return;
        }

        _depenses = result.Value.ToList();
        loading = false;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && grid is not null && !_gridInitialized)
        {
            _gridInitialized = true;
            await grid.RefreshDataAsync();
        }
    }

    private string GetRowStyle(DepenseFixeDto item)
        => item.EstDomiciliee ? "background-color: #d4edda;" : "";
}