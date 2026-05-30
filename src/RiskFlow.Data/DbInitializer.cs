using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;

namespace RiskFlow.Data;

/// <summary>
/// Prépare la base au démarrage : applique les migrations, sème les catégories par défaut
/// et garantit l'existence d'au moins une analyse pour ne pas démarrer sur un écran vide.
/// </summary>
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

        if (!await context.Analyses.AnyAsync(ct))
        {
            context.Analyses.Add(new Analysis
            {
                Name = "Analyse 1",
                ModelKey = RiskMatrixModels.Default.Key,
                SortOrder = 0,
            });

            await context.SaveChangesAsync(ct);
        }
    }
}
