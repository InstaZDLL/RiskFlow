using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;
using RiskFlow.Data;

namespace RiskFlow.ViewModels;

/// <summary>Pilote l'écran du registre des risques : chargement, ajout et suppression.</summary>
public partial class RisksViewModel(IDbContextFactory<RiskFlowDbContext> dbFactory) : ObservableObject
{
    public ObservableCollection<Risk> Risks { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteRiskCommand))]
    public partial Risk? SelectedRisk { get; set; }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db.Risks
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.RiskNumber)
            .ToListAsync();

        Risks.Clear();
        foreach (var risk in items)
            Risks.Add(risk);
    }

    [RelayCommand]
    private async Task AddRiskAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var nextNumber = Risks.Count == 0 ? 1 : Risks.Max(r => r.RiskNumber) + 1;
        var nextOrder = Risks.Count == 0 ? 0 : Risks.Max(r => r.SortOrder) + 1;

        var risk = new Risk
        {
            RiskNumber = nextNumber,
            Title = $"Nouveau risque {nextNumber}",
            SortOrder = nextOrder,
        };

        db.Risks.Add(risk);
        await db.SaveChangesAsync();

        Risks.Add(risk);
        SelectedRisk = risk;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRisk))]
    private async Task DeleteRiskAsync(Risk? risk)
    {
        risk ??= SelectedRisk;
        if (risk is null)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Risks.Remove(risk);
        await db.SaveChangesAsync();

        Risks.Remove(risk);
    }

    private bool CanDeleteRisk(Risk? risk) => (risk ?? SelectedRisk) is not null;
}
