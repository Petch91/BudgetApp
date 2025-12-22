using Datas.Tools;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Datas.Interfaces;

public interface IDepenseFixeService : IReadDepenseFixe, IWriteDepenseFixe
{
    
}

public interface IReadDepenseFixe : IReadRepository<DepenseFixeDto>
{
    Task<Result<IReadOnlyList<DepenseFixeDto>>> GetDepenseFixes();
}
public interface IWriteDepenseFixe : IWriteRepository<DepenseFixeDto, DepenseFixeForm>
{
    Task<Result> ChangeVuRappel(int id);
    Task<Result> ChangeCategorie(int depenseId, int categorieId);
    Task<Result> ChangeBeginDate(int id, DateTime beginDate);
}