using Entities.Dtos;
using Entities.Forms;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpCategorie
{
    Task<IEnumerable<CategorieDto>> GetCategories();
    Task<CategorieDto?> Add(CategorieForm categorieForm);
    Task<bool> Update(int id, CategorieForm categorieForm);
    Task<bool> Delete(int id);
}