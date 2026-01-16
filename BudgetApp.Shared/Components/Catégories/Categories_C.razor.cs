using BlazorBootstrap;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Front_BudgetApp.Services.Notifications;
using Microsoft.AspNetCore.Components;

namespace BudgetApp.Shared.Components.Catégories;

public partial class Categories_C : ComponentBase
{
    [Inject] public IHttpCategorie HttpCategorie { get; set; } = default!;
    [Inject] public IAppToastService ToastService { get; set; } = default!;

    private List<CategorieDto> _categories = [];
    private CategorieDto? selectedCategory;

    private Modal modal = default!;

    // Pagination
    private const int PageSize = 10;
    private int CurrentPage { get; set; } = 1;
    private int TotalPages => (int)Math.Ceiling(_categories.Count / (double)PageSize);

    /* =======================
     * INIT
     * ======================= */

    protected override async Task OnInitializedAsync()
    {
        var result = await HttpCategorie.GetCategories();

        if (result.IsFailed)
        {
            ToastService.Error("Erreur lors de la récupération des catégories\n" + result.Errors.First().Message,
                "Initialisation");

            return;
        }

        _categories = result.Value.ToList();
        ToastService.Success("Recuperation des catégories reussie");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            ToastService.ExecuteQueue();
        }
    }

    /* =======================
     * SELECTION & PAGINATION
     * ======================= */

    private void SelectCategory(CategorieDto categorie)
    {
        selectedCategory = selectedCategory?.Id == categorie.Id ? null : categorie;
    }

    private IEnumerable<CategorieDto> GetPagedCategories()
    {
        return _categories
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
    }

    private void GoToPage(int page)
    {
        CurrentPage = page;
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1) CurrentPage--;
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages) CurrentPage++;
    }

    /* =======================
     * ADD
     * ======================= */

    private async Task Add()
    {
        var parameters = new Dictionary<string, object>
        {
            ["SelectedCategory"] = new CategorieForm(),
            ["SubmitOk"] = EventCallback.Factory.Create<CategorieForm>(this, AddInDb),
            ["Cancelled"] = EventCallback.Factory.Create(this, () => modal.HideAsync())
        };

        await modal.ShowAsync<CategorieForm_C>(
            title: "Ajout d'une catégorie",
            parameters: parameters);
    }

    private async Task AddInDb(CategorieForm form)
    {
        var result = await HttpCategorie.Add(form);

        if (result.IsFailed)
        {
            ToastService.Error("Erreur lors de l'ajout de la catégorie", "Ajout en DB");
            await modal.HideAsync();
            ToastService.ExecuteQueue();
            return;
        }

        _categories.Add(result.Value);
        ToastService.Success("L'ajout de la catégorie à reussi", "Ajout en DB");
        ToastService.ExecuteQueue();
        await modal.HideAsync();
    }

    /* =======================
     * UPDATE
     * ======================= */

    private async Task Update()
    {
        if (selectedCategory is null)
            return;

        var parameters = new Dictionary<string, object>
        {
            ["SelectedCategory"] = new CategorieForm
            {
                Name = selectedCategory.Name,
                Icon = selectedCategory.Icon
            },
            ["SubmitOk"] = EventCallback.Factory.Create<CategorieForm>(this, UpdateInDb),
            ["Cancelled"] = EventCallback.Factory.Create(this, () => modal.HideAsync())
        };

        await modal.ShowAsync<CategorieForm_C>(
            title: "Modification d'une catégorie",
            parameters: parameters);
    }

    private async Task UpdateInDb(CategorieForm form)
    {
        if (selectedCategory is null)
            return;

        var result = await HttpCategorie.Update(selectedCategory.Id, form);

        if (result.IsFailed)
        {
            ToastService.Error("Erreur lors de l'update de la catégorie", "Mise à jour en DB");
            await modal.HideAsync();
            ToastService.ExecuteQueue();
            return;
        }

        var updated = new CategorieDto(
            selectedCategory.Id,
            form.Name,
            form.Icon
        );

        var index = _categories.IndexOf(selectedCategory);
        _categories[index] = updated;

        selectedCategory = updated;
        ToastService.Success("La catégorie a été mise à jour", "Mise à jour en DB");
        ToastService.ExecuteQueue();
        await modal.HideAsync();
    }

    /* =======================
     * DELETE
     * ======================= */

    private async Task Delete()
    {
        if (selectedCategory is null)
            return;

        var result = await HttpCategorie.Delete(selectedCategory.Id);

        if (result.IsFailed)
        {
            ToastService.Error("Erreur lors de la suppression de la catégorie", "Suppression en DB");
            ToastService.ExecuteQueue();
            return;
        }

        _categories.Remove(selectedCategory);
        selectedCategory = null;

        ToastService.Success("La catégorie a été supprimée", "Suppression en DB");
        ToastService.ExecuteQueue();
    }
}