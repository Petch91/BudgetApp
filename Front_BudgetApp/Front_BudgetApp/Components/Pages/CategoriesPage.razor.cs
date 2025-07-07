using BlazorBootstrap;
using BudgetApp.Shared.Components;
using Entities.Forms;
using Microsoft.AspNetCore.Components;

namespace Front_BudgetApp.Components.Pages;

public partial class CategoriesPage : ComponentBase
{
    private Modal modal = default!;

    private async Task Add()
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("SelectedCategory", new CategorieForm());
        await modal.ShowAsync<CategorieForm_C>(title: "AddCategorie", parameters: parameters);
    }
}