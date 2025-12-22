using BlazorBootstrap;
using BudgetApp.Shared.Tools;
using Entities.Contracts.Forms;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Catégories;

public partial class CategorieForm_C
{
    [Parameter] public CategorieForm? SelectedCategory { get; set; }
    
    [Parameter] public EventCallback<CategorieForm> SubmitOk { get; set; }
    [Parameter] public EventCallback Cancelled { get; set; }

    private List<BootIcone> BootstrapIcons = BootstrapIconList.Icons;

    
    protected override void OnParametersSet()
    {
        SelectedCategory ??= new CategorieForm();
    }
    
    private async Task<AutoCompleteDataProviderResult<BootIcone>> IconesDataProvider(AutoCompleteDataProviderRequest<BootIcone> request)
    {
        var test = request.ApplyTo(BootstrapIcons);
        return await Task.FromResult(request.ApplyTo(BootstrapIcons));
    }


    private async Task SaveCategory()
    {
        await SubmitOk.InvokeAsync(SelectedCategory);
    }

    private async Task Quit()
    {
        await Cancelled.InvokeAsync();
    }

    private void OnAutoChangedIcone(BootIcone icone)
    {
        SelectedCategory.Icon = icone is not null ? icone.IconeName : String.Empty;
    }
}