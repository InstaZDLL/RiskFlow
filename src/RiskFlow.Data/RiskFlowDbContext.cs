using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;

namespace RiskFlow.Data;

/// <summary>
/// Contexte EF Core / SQLite de RiskFlow. Une analyse possède plusieurs risques
/// (suppression en cascade) ; les catégories sont partagées entre analyses.
/// </summary>
public class RiskFlowDbContext(DbContextOptions<RiskFlowDbContext> options) : DbContext(options)
{
    public DbSet<Analysis> Analyses => Set<Analysis>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<RiskCategory> RiskCategories => Set<RiskCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analysis>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired();
            entity.Property(a => a.ModelKey).IsRequired();
            entity.HasIndex(a => a.SortOrder);
        });

        modelBuilder.Entity<Risk>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title).IsRequired();
            entity.Property(r => r.Category).IsRequired();
            entity.HasIndex(r => r.AnalysisId);
            entity.HasIndex(r => r.SortOrder);
            entity.HasOne(r => r.Analysis)
                .WithMany(a => a.Risks)
                .HasForeignKey(r => r.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RiskCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();
        });
    }

    public override int SaveChanges()
    {
        StampTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>Renseigne CreatedAt/UpdatedAt sur les entités ajoutées ou modifiées.</summary>
    private void StampTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not (Analysis or Risk or RiskCategory))
                continue;

            if (entry.State == EntityState.Added)
                entry.Property(nameof(Risk.CreatedAt)).CurrentValue = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(nameof(Risk.UpdatedAt)).CurrentValue = now;
        }
    }
}
