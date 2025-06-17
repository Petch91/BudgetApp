using Entities.Dtos;
using Entities.Forms;

namespace Datas.Services.Interfaces;

public interface ITranscation : IReadTranscation, IWriteTranscation
{
    
}
public interface IReadTranscation : IReadRepository<TransactionVariableDto>
{
    Task<IEnumerable<TransactionVariableDto>> GetRevenuesByMonth(int month);
    Task<IEnumerable<TransactionVariableDto>> GetDepensesByMonth(int month);
}
public interface IWriteTranscation : IWriteRepository<TransactionVariableForm>
{
    Task<bool> ChangeCategorie(int id, CategorieDto categorie);
}