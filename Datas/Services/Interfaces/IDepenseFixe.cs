using Entities.Dtos;
using Entities.Forms;
using Entities.Models;

namespace Datas.Services.Interfaces;

public interface IDepenseFixe : IReadDepenseFixe, IWriteDepenseFixe
{
    
}

public interface IReadDepenseFixe : IReadRepository<DepenseFixeDto>
{
    Task<IEnumerable<DepenseFixeDto>> GetDepenseFixes();
}
public interface IWriteDepenseFixe : IWriteRepository<DepenseFixeForm>
{
    Task<bool> MarkVuRappel(int id);
    Task<bool> ChangeCategorie(int id, CategorieDto categorie);
    Task<bool> ChangeDateDue(int id, DateTime due);
}