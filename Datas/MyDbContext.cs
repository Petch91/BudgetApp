using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Datas;

public class MyDbContext : DbContext
{
    public DbSet<DepenseFixe> DepenseFixes => Set<DepenseFixe>();
    public DbSet<TransactionVariable> TransactionsVariables => Set<TransactionVariable>();
    public DbSet<Rappel> Rappels => Set<Rappel>();
    public DbSet<Categorie> Categories => Set<Categorie>();
    
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {

    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entityDepenseFixe = modelBuilder.Entity<DepenseFixe>();
        
        entityDepenseFixe.ToTable("DepenseFixes", tb => tb.HasTrigger("TG_UpdateDepenseFix"));
        
        entityDepenseFixe
            .Property(p => p.Intitule).HasMaxLength(150);
        
        entityDepenseFixe
            .Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        
        entityDepenseFixe
            .Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityDepenseFixe.HasOne(t => t.Categorie).WithMany(c => c.Transactions as IEnumerable<DepenseFixe>);

        
        var entityTransaction = modelBuilder.Entity<TransactionVariable>();
        
        entityTransaction.ToTable("TransactionsVariables", tb => tb.HasTrigger("TG_UpdateTransaction"));
        
        entityTransaction
            .Property(p => p.Intitule).HasMaxLength(150);
        
        entityTransaction
            .Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        
        entityTransaction
            .Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");

        entityTransaction.HasOne(t => t.Categorie).WithMany(c => c.Transactions as IEnumerable<TransactionVariable>);

        var entityCategorie = modelBuilder.Entity<Categorie>();
        
        entityCategorie.ToTable("Categories", tb => tb.HasTrigger("TG_UpdateCategorie"));
        
        entityCategorie
            .Property(p => p.Name).HasMaxLength(50);
        
        entityCategorie
            .Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        
        entityCategorie
            .Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        
        var entityRappel = modelBuilder.Entity<Rappel>();
        
        entityRappel.ToTable("Rappels", tb => tb.HasTrigger("TG_UpdateRappel"));
        
        entityRappel
            .Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
        
        entityRappel
            .Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        entityRappel.HasOne(r => r.DepenseFixe).WithMany(d => d.Rappels);
    }
}