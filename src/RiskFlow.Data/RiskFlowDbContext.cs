using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RiskFlow.Core.Risks;

namespace RiskFlow.Data;

/// <summary>
/// Contexte EF Core / SQLite de RiskFlow. Les gravités et probabilités sont stockées
/// sous leur libellé français et les niveaux dérivés ne sont pas persistés (recalculés).
/// </summary>
public class RiskFlowDbContext(DbContextOptions<RiskFlowDbContext> options) : DbContext(options)
{
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<RiskCategory> RiskCategories => Set<RiskCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var severityConverter = new ValueConverter<Severity, string>(
            v => v.ToFr(),
            v => RiskLabels.ParseSeverity(v));

        var likelihoodConverter = new ValueConverter<Likelihood, string>(
            v => v.ToFr(),
            v => RiskLabels.ParseLikelihood(v));

        modelBuilder.Entity<Risk>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title).IsRequired();
            entity.Property(r => r.Category).IsRequired();
            entity.Property(r => r.BeforeSeverity).HasConversion(severityConverter);
            entity.Property(r => r.BeforeLikelihood).HasConversion(likelihoodConverter);
            entity.Property(r => r.AfterSeverity).HasConversion(severityConverter);
            entity.Property(r => r.AfterLikelihood).HasConversion(likelihoodConverter);
            entity.HasIndex(r => r.SortOrder);
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
            if (entry.Entity is not (Risk or RiskCategory))
                continue;

            if (entry.State == EntityState.Added)
                entry.Property(nameof(Risk.CreatedAt)).CurrentValue = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(nameof(Risk.UpdatedAt)).CurrentValue = now;
        }
    }
}
