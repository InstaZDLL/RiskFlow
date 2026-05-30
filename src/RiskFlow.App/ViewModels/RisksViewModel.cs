using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;
using RiskFlow.Data;

namespace RiskFlow.ViewModels;

/// <summary>
/// Pilote l'écran du registre des risques pour l'analyse courante : chargement, ajout,
/// suppression. Les niveaux de risque dépendent du modèle de matrice de l'analyse.
/// </summary>
public partial class RisksViewModel(IDbContextFactory<RiskFlowDbContext> dbFactory) : ObservableObject
{
    private Analysis? _analysis;
    private RiskMatrixModel _model = RiskMatrixModels.Default;

    public ObservableCollection<RiskRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteRiskCommand))]
    public partial RiskRowViewModel? SelectedRow { get; set; }

    [ObservableProperty]
    public partial string AnalysisName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ModelName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddRiskCommand))]
    public partial bool HasAnalysis { get; set; }

    /// <summary>Fixe l'analyse affichée et recharge ses risques.</summary>
    public async Task SetAnalysisAsync(Analysis? analysis)
    {
        _analysis = analysis;
        _model = analysis?.Model ?? RiskMatrixModels.Default;
        AnalysisName = analysis?.Name ?? string.Empty;
        ModelName = _model.Name;
        HasAnalysis = analysis is not null;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Rows.Clear();
        SelectedRow = null;
        if (_analysis is null)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db.Risks
            .Where(r => r.AnalysisId == _analysis.Id)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.RiskNumber)
            .ToListAsync();

        foreach (var risk in items)
            Rows.Add(new RiskRowViewModel(risk, _model));
    }

    [RelayCommand(CanExecute = nameof(HasAnalysis))]
    private async Task AddRiskAsync()
    {
        if (_analysis is null)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();

        var nextNumber = Rows.Count == 0 ? 1 : Rows.Max(r => r.RiskNumber) + 1;
        var nextOrder = Rows.Count == 0 ? 0 : Rows.Max(r => r.Model.SortOrder) + 1;

        var risk = new Risk
        {
            AnalysisId = _analysis.Id,
            RiskNumber = nextNumber,
            Title = $"Nouveau risque {nextNumber}",
            SortOrder = nextOrder,
        };

        db.Risks.Add(risk);
        await db.SaveChangesAsync();

        var row = new RiskRowViewModel(risk, _model);
        Rows.Add(row);
        SelectedRow = row;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRisk))]
    private async Task DeleteRiskAsync(RiskRowViewModel? row)
    {
        row ??= SelectedRow;
        if (row is null)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Risks.Remove(row.Model);
        await db.SaveChangesAsync();

        Rows.Remove(row);
    }

    private bool CanDeleteRisk(RiskRowViewModel? row) => (row ?? SelectedRow) is not null;
}
