using Entities.Contracts.Dtos;
using FluentResults;

namespace BudgetApp.Mobile.Services.Hybrid;

public interface IRapportHybridService
{
    Task<Result<MobileRapportDto>> GetRapportAsync(int year, int month);
}

public class MobileRapportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenus { get; set; }
    public decimal TotalDepenses { get; set; }
    public decimal Solde => TotalRevenus - TotalDepenses;
    public List<MobileTransactionDto> Transactions { get; set; } = [];
    public bool IsOffline { get; set; }
}

public class MobileTransactionDto
{
    public int LocalId { get; set; }
    public int? ServerId { get; set; }
    public string Intitule { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public DateTime Date { get; set; }
    public Entities.Domain.Models.TransactionType TransactionType { get; set; }
    public string? CategorieName { get; set; }
    public bool IsPending { get; set; }
}
