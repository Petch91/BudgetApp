using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;

namespace BudgetApp.Shared.Interfaces.Http;

public interface IHttpTransaction
{
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetByMonth(int month, int year);
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetRevenuesByMonth(int month, int year);
    Task<Result<IReadOnlyList<TransactionVariableDto>>> GetDepensesByMonth(int month, int year);
    Task<Result<TransactionVariableDto>> Add(TransactionVariableForm form);
    Task<Result> Update(int id, TransactionVariableForm form);
    Task<Result> Delete(int id);
}
