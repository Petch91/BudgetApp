using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components;

public partial class Categories_C : ComponentBase
{
    [Inject] public IHttpCategorie  HttpCategorie { get; set; }
    
    private IEnumerable<CategorieDto> _categories =  new List<CategorieDto>();
    private int _selectedCategoryId ;
    
    private Grid<CategorieDto> grid;
    private HashSet<CategorieDto> selectedCategories = new HashSet<CategorieDto>();
    
    protected override async Task OnInitializedAsync()
    {
        _categories =  await HttpCategorie.GetCategories();
        StateHasChanged();
    }
    
    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender)
    //     {
    //         _categories = await HttpCategorie.GetCategories();
    //         StateHasChanged();
    //     }
    // }
    
    private Task OnSelectedItemsChanged(HashSet<CategorieDto> categories)
    {
        if (selectedCategories.Count > 1 ) selectedCategories.Remove(selectedCategories.First());
        selectedCategories = categories is not null && categories.Any() ? categories : new();
        //StateHasChanged();
        return Task.CompletedTask;
    }
}