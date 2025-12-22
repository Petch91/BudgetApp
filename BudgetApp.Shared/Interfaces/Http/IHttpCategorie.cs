using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpCategorie
{
    Task<Result<IReadOnlyList<CategorieDto>>> GetCategories();
    Task<Result<CategorieDto>> Add(CategorieForm form);
    Task<Result> Update(int id, CategorieForm form);
    Task<Result> Delete(int id);

}