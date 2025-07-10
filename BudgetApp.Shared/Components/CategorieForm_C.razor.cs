using BudgetApp.Shared.Tools;
using Entities.Forms;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components;

public partial class CategorieForm_C
{
    [Parameter] public CategorieForm? SelectedCategory { get; set; }

    private List<string> BootstrapIcons = BootstrapIconList.Icons;

    protected override void OnParametersSet()
    {
        SelectedCategory = SelectedCategory ?? new CategorieForm();
    }

    private void SaveCategory()
    {

    }

    private void Quit()
    {
        
    }

}