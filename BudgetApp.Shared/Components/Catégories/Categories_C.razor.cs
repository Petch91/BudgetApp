using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Dtos;
using Entities.Forms;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Catégories;

public partial class Categories_C : ComponentBase
{
    [Inject] public IHttpCategorie  HttpCategorie { get; set; }
    
    private List<CategorieDto> _categories =  new List<CategorieDto>();
    private int _selectedCategoryId ;
    
    private Grid<CategorieDto> grid;
    private HashSet<CategorieDto> selectedCategories = new HashSet<CategorieDto>();
    private CategorieDto? selectedCategory = null;
    
    protected override async Task OnInitializedAsync()
    {
        var temp = await HttpCategorie.GetCategories();
        _categories = temp.ToList();
        await grid.RefreshDataAsync();
    }
    
    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender)
    //     {
    //         _categories = await HttpCategorie.GetCategories();
    //         StateHasChanged();
    //     }
    // }
    
    private async Task<GridDataProviderResult<CategorieDto>> CategorieDataProvider(GridDataProviderRequest<CategorieDto> request)
    {
        return await Task.FromResult(request.ApplyTo(_categories));
    }
    
    private Task OnSelectedItemsChanged(HashSet<CategorieDto> categories)
    {
        if (selectedCategories.Count > 1 ) selectedCategories.Remove(selectedCategories.First());
        selectedCategories = categories is not null && categories.Any() ? categories : new();
        selectedCategory =  selectedCategories.Count == 1 ? selectedCategories.First() : null;
        return Task.CompletedTask;
    }
    
    private Modal modal = default!;

    private async Task Add()
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("SelectedCategory", new CategorieForm());
        parameters.Add("SubmitOk", EventCallback.Factory.Create<CategorieForm>(this, AddInDb));
        parameters.Add("Cancelled", EventCallback.Factory.Create(this, async () => await modal.HideAsync()));
        await modal.ShowAsync<CategorieForm_C>(title: "Ajout d'une catégorie", parameters: parameters);
    }
    
    private async void AddInDb(CategorieForm categorie)
    {
        var cat = await HttpCategorie.Add(categorie);
        if (cat is not null) 
        {
            _categories.Add(cat);
            await grid.RefreshDataAsync();
        }
        else Console.WriteLine("Erreur add cat front"); //faire un toast
        await modal.HideAsync();
        StateHasChanged();
        
    }
    private async Task Update()
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("SelectedCategory", new CategorieForm{ Name = selectedCategory?.Name ?? "", Icon = selectedCategory?.Icon ?? ""} );
        parameters.Add("SubmitOk", EventCallback.Factory.Create<CategorieForm>(this, UpdateInDb));
        parameters.Add("Cancelled", EventCallback.Factory.Create(this, async () => await modal.HideAsync()));
        await modal.ShowAsync<CategorieForm_C>(title: "Ajout d'une catégorie", parameters: parameters);
    }
    
    private async void UpdateInDb(CategorieForm categorie)
    {
        if (await HttpCategorie.Update(selectedCategory.Id, categorie))
        {
            var cat = new CategorieDto { Id = selectedCategory.Id, Name = categorie.Name, Icon = categorie.Icon };
            var index =  _categories.IndexOf(selectedCategory);
            _categories[index] = cat;
            await grid.RefreshDataAsync();
        }
        else Console.WriteLine("Erreur update cat front"); //faire un toast
        await modal.HideAsync();
        StateHasChanged();
        
    }

    private async Task Delete()
    {
        if (await HttpCategorie.Delete(selectedCategories.First().Id))
        {
            _categories.Remove(selectedCategories.First());
            selectedCategories.Clear();
            await grid.RefreshDataAsync();
        }
    }

}