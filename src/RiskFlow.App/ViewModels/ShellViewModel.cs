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
/// Pilote la barre latérale : liste des analyses, analyse sélectionnée, création et
/// suppression. La sélection répercute le rechargement vers le <see cref="RisksViewModel"/>.
/// </summary>
public partial class ShellViewModel(IDbContextFactory<RiskFlowDbContext> dbFactory, RisksViewModel risks) : ObservableObject
{
    /// <summary>ViewModel du registre, alimenté selon l'analyse sélectionnée.</summary>
    public RisksViewModel Risks => risks;

    public ObservableCollection<Analysis> Analyses { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteAnalysisCommand))]
    public partial Analysis? SelectedAnalysis { get; set; }

    /// <summary>Charge les analyses et sélectionne la première.</summary>
    public async Task LoadAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db.Analyses
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Id)
            .ToListAsync();

        Analyses.Clear();
        foreach (var analysis in items)
            Analyses.Add(analysis);

        SelectedAnalysis = Analyses.FirstOrDefault();
    }

    partial void OnSelectedAnalysisChanged(Analysis? value)
    {
        // Fire-and-forget : le rechargement met à jour les collections observées par l'UI.
        _ = risks.SetAnalysisAsync(value);
    }

    /// <summary>Crée une analyse, l'ajoute à la liste et la sélectionne.</summary>
    public async Task<Analysis> CreateAnalysisAsync(string name, string modelKey)
    {
        var nextOrder = Analyses.Count == 0 ? 0 : Analyses.Max(a => a.SortOrder) + 1;

        var analysis = new Analysis
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Analyse {Analyses.Count + 1}" : name.Trim(),
            ModelKey = RiskMatrixModels.Get(modelKey).Key,
            SortOrder = nextOrder,
        };

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Analyses.Add(analysis);
        await db.SaveChangesAsync();

        Analyses.Add(analysis);
        SelectedAnalysis = analysis;
        return analysis;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAnalysis))]
    private async Task DeleteAnalysisAsync(Analysis? analysis)
    {
        analysis ??= SelectedAnalysis;
        if (analysis is null)
            return;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Analyses.Remove(analysis);
        await db.SaveChangesAsync();

        var index = Analyses.IndexOf(analysis);
        Analyses.Remove(analysis);
        SelectedAnalysis = Analyses.ElementAtOrDefault(index) ?? Analyses.LastOrDefault();
    }

    private bool CanDeleteAnalysis(Analysis? analysis) => (analysis ?? SelectedAnalysis) is not null;
}
