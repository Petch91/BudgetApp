using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;

namespace Datas.Services.Interfaces;

public interface ITranscationService : IReadTranscation, IWriteTranscation
{
    
}
public interface IReadTranscation : IReadRepository<TransactionVariableDto>
{
    Task<IEnumerable<TransactionVariableDto>> GetRevenuesByMonth(int month);
    Task<IEnumerable<TransactionVariableDto>> GetDepensesByMonth(int month);
}
public interface IWriteTranscation : IWriteRepository<TransactionVariableForm>
{
    Task<Result> ChangeCategorie(int transactionId, int categorieId);
}