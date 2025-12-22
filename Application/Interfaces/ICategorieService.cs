using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Datas.Interfaces;

public interface ICategorieService : IReadCategorie, IWriteCategorie
{
    
}
public interface IReadCategorie : IReadRepository<CategorieDto>
{
    public Task<Result<IReadOnlyList<CategorieDto>>> GetCategories();
}
public interface IWriteCategorie : IWriteRepository<CategorieDto, CategorieForm>
{
    
}