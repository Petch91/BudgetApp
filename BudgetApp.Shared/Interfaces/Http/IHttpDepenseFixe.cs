using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpDepenseFixe
{
    Task<Result<IReadOnlyList<DepenseFixeDto>>> GetDepenses();
    Task<Result<DepenseFixeDto>> Add(DepenseFixeForm depenseForm);
    Task<Result> Update(int id, DepenseFixeForm depenseForm);
    Task<Result> Delete(int id);
    Task<Result> ChangeVuRappel(int id);
}