using Datas.Services.Interfaces;
using Entities.Dtos;
using Entities.Forms;


namespace Datas.Services;

public class DepenseFixeService : IDepenseFixe
{
    public Task<DepenseFixeDto> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DepenseFixeDto>> GetDepenseFixes()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Add(DepenseFixeForm entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Update(int id, DepenseFixeForm entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkVuRappel(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeCategorie(int id, CategorieDto categorie)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeDateDue(int id, DateTime due)
    {
        throw new NotImplementedException();
    }
}