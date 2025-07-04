using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Models;

namespace Datas.Services.Interfaces;

public interface IDepenseFixeService : IReadDepenseFixe, IWriteDepenseFixe
{
    
}

public interface IReadDepenseFixe : IReadRepository<DepenseFixeDto>
{
    Task<IEnumerable<DepenseFixeDto>> GetDepenseFixes();
}
public interface IWriteDepenseFixe : IWriteRepository<DepenseFixeForm>
{
    Task<bool> ChangeVuRappel(int id);
    Task<Result> ChangeCategorie(int depenseId, int categorieId);
    Task<bool> ChangeBeginDate(int id, DateTime beginDate);
}