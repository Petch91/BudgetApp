using Entities.Forms;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components;

public partial class CategorieForm_C
{
    [Parameter] public CategorieForm? SelectedCategory { get; set; }
    
    private List<string> BootstrapIcons = new()
    {
        "bi-alarm", "bi-app", "bi-basket", "bi-battery", "bi-calendar", "bi-camera", 
        "bi-cart", "bi-cash", "bi-chat", "bi-clipboard", "bi-cloud", "bi-cpu", "bi-credit-card",
        "bi-emoji-smile", "bi-envelope", "bi-file-earmark", "bi-gem", "bi-geo", "bi-globe",
        "bi-house", "bi-lightning", "bi-lock", "bi-moon", "bi-palette", "bi-piggy-bank", 
        "bi-printer", "bi-rocket", "bi-tools", "bi-wallet", "bi-wifi"
    };

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