using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Datas;

public class MyDbContext : DbContext
{
    public DbSet<DepenseFixe> DepenseFixes => Set<DepenseFixe>();
    public DbSet<DepenseVariable> DepenseVariables => Set<DepenseVariable>();
    public DbSet<Revenu> Revenues => Set<Revenu>();
    
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
        
        var entityDepenseVariable = modelBuilder.Entity<DepenseVariable>();
        var entityRevenu = modelBuilder.Entity<Revenu>();
    }
}