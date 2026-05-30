using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;

namespace RiskFlow.Data;

/// <summary>Prépare la base au démarrage : applique les migrations et sème les catégories par défaut.</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(RiskFlowDbContext context, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        if (!await context.RiskCategories.AnyAsync(ct))
        {
            var order = 0;
            foreach (var name in RiskCategory.Defaults)
                context.RiskCategories.Add(new RiskCategory { Name = name, SortOrder = order++ });

            await context.SaveChangesAsync(ct);
        }
    }
}
