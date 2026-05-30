using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RiskFlow.Data;

/// <summary>
/// Fabrique utilisée uniquement par les outils EF Core (dotnet ef migrations).
/// La chaîne de connexion est factice : aucune base n'est touchée à la génération.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RiskFlowDbContext>
{
    public RiskFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RiskFlowDbContext>()
            .UseSqlite("Data Source=riskflow.design.db")
            .Options;
        return new RiskFlowDbContext(options);
    }
}
