using BudgetApp.Mobile.Models.Local;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Mobile.Services.Offline;

public class LocalDbContext : DbContext
{
    public DbSet<LocalTransaction> Transactions { get; set; } = null!;
    public DbSet<LocalCategorie> Categories { get; set; } = null!;
    public DbSet<SyncMetadata> SyncMetadata { get; set; } = null!;

    private readonly string _dbPath;

    public LocalDbContext()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "budgetapp.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LocalTransaction
        modelBuilder.Entity<LocalTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.LocalId)
                .IsUnique();

            entity.HasIndex(e => e.ServerId);

            entity.HasIndex(e => e.SyncState);

            entity.HasIndex(e => new { e.UserId, e.Date });

            entity.Property(e => e.Intitule)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Montant)
                .HasColumnType("REAL");

            entity.Property(e => e.LocalId)
                .IsRequired()
                .HasMaxLength(36);

            entity.HasOne(e => e.Categorie)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CategorieId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LocalCategorie
        modelBuilder.Entity<LocalCategorie>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Icon)
                .HasMaxLength(50);
        });

        // SyncMetadata
        modelBuilder.Entity<SyncMetadata>(entity =>
        {
            entity.HasKey(e => e.EntityType);

            entity.Property(e => e.EntityType)
                .HasMaxLength(50);
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await Database.EnsureCreatedAsync();
    }
}
