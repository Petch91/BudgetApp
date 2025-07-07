using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Datas;

public class MyDbContext : DbContext
{
    public DbSet<DepenseFixe> DepenseFixes => Set<DepenseFixe>();
    public DbSet<TransactionVariable> TransactionsVariables => Set<TransactionVariable>();
    public DbSet<Rappel> Rappels => Set<Rappel>();
    public DbSet<Categorie> Categories => Set<Categorie>();
    public DbSet<DepenseMois> DepensesMois => Set<DepenseMois>();
    public DbSet<DepenseDueDate> depenseDueDates => Set<DepenseDueDate>();

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Inheritance TPH sur Transaction
        modelBuilder.Entity<Transaction>()
            .ToTable("Transactions", tb => tb.HasTrigger("TG_UpdateTransaction"))
            .HasDiscriminator<string>("TransactionTable")
            .HasValue<DepenseFixe>("Fixe")
            .HasValue<TransactionVariable>("Variable");

        var entityDepenseFixe = modelBuilder.Entity<DepenseFixe>();

        // ❌ NE PAS appeler .ToTable ici (sinon repasse en TPT)
        entityDepenseFixe
            .Property(p => p.Intitule).HasMaxLength(150);
        entityDepenseFixe.Property(p => p.CategorieId).HasDefaultValue(1);
        entityDepenseFixe.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseFixe.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseFixe.HasOne(t => t.Categorie)
            .WithMany(c => c.Transactions as IEnumerable<DepenseFixe>);

        var entityTransaction = modelBuilder.Entity<TransactionVariable>();

        // ❌ NE PAS appeler .ToTable ici
        entityTransaction
            .Property(p => p.Intitule).HasMaxLength(150);
        entityTransaction.Property(p => p.CategorieId).HasDefaultValue(1);
        entityTransaction.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityTransaction.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityTransaction.HasOne(t => t.Categorie)
            .WithMany(c => c.Transactions as IEnumerable<TransactionVariable>);

        var entityCategorie = modelBuilder.Entity<Categorie>();
        entityCategorie.ToTable("Categories", tb => tb.HasTrigger("TG_UpdateCategorie"));
        entityCategorie.Property(p => p.Name).HasMaxLength(50);
        entityCategorie.Property(p => p.Icon).HasMaxLength(25);
        entityCategorie.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityCategorie.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityCategorie.HasData(new Categorie { Id = 1, Name = "NoCategory" });

        var entityRappel = modelBuilder.Entity<Rappel>();
        entityRappel.ToTable("Rappels", tb => tb.HasTrigger("TG_UpdateRappel"));
        entityRappel.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityRappel.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityRappel.HasOne(r => r.DepenseFixe)
            .WithMany(d => d.Rappels);
        
        var entityDepenseMois = modelBuilder.Entity<DepenseMois>();
        entityDepenseMois.ToTable("DepenseMois", tb => tb.HasTrigger("TG_UpdateDepenseMois"));
        entityDepenseMois.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseMois.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        
        var entityDepenseDueDates = modelBuilder.Entity<DepenseDueDate>();
        entityDepenseDueDates.ToTable("DepenseDueDates", tb => tb.HasTrigger("TG_UpdateDepenseDueDates"));
        entityDepenseDueDates.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseDueDates.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseDueDates.HasOne(p => p.Depense).WithMany(x => x.DueDates);
    }
}