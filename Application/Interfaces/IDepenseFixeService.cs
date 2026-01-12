using Application.Tools;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;

namespace Application.Interfaces;

public interface IDepenseFixeService : IReadDepenseFixe, IWriteDepenseFixe
{
    void GenerateNextDates(DepenseFixe depense, DateTime startDate);
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