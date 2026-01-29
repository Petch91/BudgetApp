using Application.Tools;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace Application.Interfaces;

public interface ITranscationService : IReadTranscation, IWriteTranscation
{
    
}
public interface IReadTranscation : IReadRepository<TransactionVariableDto>
{
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetRevenuesByMonth(int month, int userId);
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetDepensesByMonth(int month, int userId);
}
public interface IWriteTranscation : IWriteRepository<TransactionVariableDto, TransactionVariableForm>
{
    Task<Result> ChangeCategorie(int transactionId, int categorieId);
}