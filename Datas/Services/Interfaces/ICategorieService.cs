using Entities.Dtos;
using Entities.Forms;
using Entities.Models;

namespace Datas.Services.Interfaces;

public interface ICategorieService : IReadCategorie, IWriteCategorie
{
    
}
public interface IReadCategorie : IReadRepository<CategorieDto>
{
    Task<IEnumerable<CategorieDto>> GetCategories();
}
public interface IWriteCategorie : IWriteRepository<CategorieForm>
{
    
}