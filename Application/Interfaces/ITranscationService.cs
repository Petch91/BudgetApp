using Datas.Tools;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Datas.Interfaces;

public interface ITranscationService : IReadTranscation, IWriteTranscation
{
    
}
public interface IReadTranscation : IReadRepository<TransactionVariableDto>
{
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetRevenuesByMonth(int month);
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetDepensesByMonth(int month);
}
public interface IWriteTranscation : IWriteRepository<TransactionVariableDto, TransactionVariableForm>
{
    Task<Result> ChangeCategorie(int transactionId, int categorieId);
}