using Application.Persistence;
using Entities.Domain.Models;
using Microsoft.EntityFrameworkCore;

public class DepenseFixeScheduler(
    IServiceScopeFactory scopeFactory,
    ILogger<DepenseFixeScheduler> logger)
    : BackgroundService
{
    private readonly ILogger<DepenseFixeScheduler> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var today = DateTime.Today;
        var retentionDays = 4;
        var deleteBefore = today.AddDays(-retentionDays);

        var depenses = await context.DepenseFixes
            .Include(d => d.DueDates)
            .Include(d => d.Rappels)
            .ToListAsync();

        foreach (var depense in depenses)
        {
            // 1️⃣ Supprimer les dates échues
            var expiredDates = depense.DueDates
                .Where(d => d.Date < deleteBefore)
                .ToList();

            context.depenseDueDates.RemoveRange(expiredDates);

            var expiredRappels = depense.Rappels
                .Where(r => r.RappelDate < deleteBefore)
                .ToList();

            context.Rappels.RemoveRange(expiredRappels);

            // 2️⃣ Vérifier s’il reste des dates futures
            var lastDate = depense.DueDates
                .OrderByDescending(d => d.Date)
                .Select(d => d.Date)
                .FirstOrDefault();

            if (lastDate < today.AddMonths(2))
            {
                GenerateNextDates(depense, lastDate);
            }
        }

        await context.SaveChangesAsync();
    }

    private void GenerateNextDates(DepenseFixe depense, DateTime startDate)
    {
        var date = startDate;

        for (int i = 0; i < 3; i++) // horizon de sécurité
        {
            date = depense.Frequence switch
            {
                Frequence.Mensuel => date.AddMonths(1),
                Frequence.Trimestriel => date.AddMonths(3),
                Frequence.Biannuel => date.AddMonths(6),
                Frequence.Annuel => date.AddYears(1),
                _ => throw new ArgumentOutOfRangeException()
            };

            depense.DueDates.Add(new DepenseDueDate { Date = date });

            if (!depense.EstDomiciliee)
            {
                foreach (var rappel in SetRappels(date, depense.ReminderDaysBefore))
                {
                    depense.Rappels.Add(rappel);
                }
            }
        }
    }

    /* =======================
     * PRIVATE LOGIC
     * ======================= */


    private static List<Rappel> SetRappels(DateTime date, int reminderDaysBefore)
        => new()
        {
            new() { RappelDate = date.AddDays(-reminderDaysBefore) },
            new() { RappelDate = date.AddDays(-1) },
            new() { RappelDate = date },
        };
}