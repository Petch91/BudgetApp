using BudgetApp.Mobile.Models.Local;
using Entities.Contracts.Forms;
using FluentResults;

namespace BudgetApp.Mobile.Services.Hybrid;

public interface ITransactionHybridService
{
    Task<Result<List<LocalTransaction>>> GetByMonthAsync(int year, int month);
    Task<Result<LocalTransaction>> CreateAsync(TransactionVariableForm form);
    Task<Result> UpdateAsync(int localId, TransactionVariableForm form);
    Task<Result> DeleteAsync(int localId);
}
