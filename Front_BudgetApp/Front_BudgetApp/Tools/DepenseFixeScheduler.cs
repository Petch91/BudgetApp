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
        var rappelRetentionDays = 5;
        var deleteRappelsBefore = today.AddDays(-rappelRetentionDays);

        var depenses = await context.DepenseFixes
            .Include(d => d.DueDates)
            .Include(d => d.Rappels)
            .ToListAsync();

        foreach (var depense in depenses)
        {
            // 1. Supprimer les rappels vus et expirés (5 jours après la date)
            var expiredRappels = depense.Rappels
                .Where(r => r.Vu && r.RappelDate < deleteRappelsBefore)
                .ToList();

            context.Rappels.RemoveRange(expiredRappels);

            // 2. Traiter les dépenses échelonnées
            if (depense.IsEchelonne && depense.EcheancesRestantes > 0)
            {
                await TraiterEchelonnement(context, depense, today);
                continue;
            }

            // 3. Vérifier s'il faut générer de nouvelles dates (horizon 2 mois)
            // Ne pas générer si la dépense a une date de fin dépassée
            if (depense.DateFin.HasValue && depense.DateFin.Value < today)
                continue;

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

    private async Task TraiterEchelonnement(MyDbContext context, DepenseFixe depense, DateTime today)
    {
        var total = depense.NombreEcheances!.Value;
        var startDate = depense.DueDates.Select(d => d.Date).Min();

        // Boucle : rattraper toutes les échéances passées d'un coup
        while (depense.EcheancesRestantes > 0)
        {
            var numero = total - depense.EcheancesRestantes.Value + 1;
            var datePaiement = startDate.AddMonths(numero - 1);

            // Arrêter si la date de paiement n'est pas encore arrivée
            if (datePaiement > today)
                break;

            // Vérifier si une transaction variable existe déjà pour ce mois
            var dejaCreeCeMois = await context.TransactionsVariables
                .AnyAsync(t =>
                    t.UserId == depense.UserId &&
                    t.Date.Month == datePaiement.Month &&
                    t.Date.Year == datePaiement.Year &&
                    t.Intitule.StartsWith(depense.Intitule + " - Echeance"));

            if (dejaCreeCeMois)
            {
                depense.EcheancesRestantes--;
                continue;
            }

            // Calculer le montant (ajuster la dernière échéance)
            decimal montant;
            if (depense.EcheancesRestantes == 1)
                montant = depense.Montant - (total - 1) * depense.MontantParEcheance!.Value;
            else
                montant = depense.MontantParEcheance!.Value;

            // Créer la TransactionVariable avec la date de paiement prévue
            var transaction = new TransactionVariable
            {
                Intitule = $"{depense.Intitule} - Echeance {numero}/{total}",
                Montant = montant,
                Date = datePaiement,
                TransactionType = TransactionType.Depense,
                CategorieId = depense.CategorieId,
                UserId = depense.UserId
            };

            context.TransactionsVariables.Add(transaction);
            depense.EcheancesRestantes--;

            _logger.LogInformation("Echeance {Numero}/{Total} créée pour {Intitule} au {Date}",
                numero, total, depense.Intitule, datePaiement.ToShortDateString());
        }
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

            // Ne pas générer de dates au-delà de DateFin
            if (depense.DateFin.HasValue && date > depense.DateFin.Value)
                break;

            depense.DueDates.Add(new DepenseDueDate
            {
                Date = date,
                MontantEffectif = depense.Montant
            });

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
