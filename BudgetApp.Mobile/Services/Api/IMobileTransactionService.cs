using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;

namespace BudgetApp.Mobile.Services.Api;

public interface IMobileTransactionService
{
    Task<Result<List<TransactionVariableDto>>> GetByMonthAsync(int year, int month);
    Task<Result<TransactionVariableDto>> CreateAsync(TransactionVariableForm form);
    Task<Result> UpdateAsync(TransactionVariableForm form);
    Task<Result> DeleteAsync(int id);
}
