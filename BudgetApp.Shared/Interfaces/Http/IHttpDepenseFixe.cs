using Entities.Dtos;
using Entities.Forms;


namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpDepenseFixe
{
    Task<IEnumerable<DepenseFixeDto>> GetDepenses();
    Task<DepenseFixeDto?> Add(DepenseFixeForm depenseForm);
    Task<bool> Update(int id, DepenseFixeForm depenseForm);
    Task<bool> Delete(int id);
    Task<bool> ChangeVuRappel(int id);
}