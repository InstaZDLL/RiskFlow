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

        // Normalisation des bases héritées : l'ancienne catégorie par défaut
        // « Conformité/LPD » est renommée « Conformité » (si la nouvelle n'existe pas déjà).
        var legacy = await context.RiskCategories.FirstOrDefaultAsync(c => c.Name == "Conformité/LPD", ct);
        if (legacy is not null && !await context.RiskCategories.AnyAsync(c => c.Name == "Conformité", ct))
        {
            // Renommage + propagation aux risques (champ dénormalisé) en une transaction.
            await using var tx = await context.Database.BeginTransactionAsync(ct);
            legacy.Name = "Conformité";
            await context.SaveChangesAsync(ct);
            await context.Risks.Where(r => r.Category == "Conformité/LPD")
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Category, "Conformité"), ct);
            await tx.CommitAsync(ct);
        }

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
