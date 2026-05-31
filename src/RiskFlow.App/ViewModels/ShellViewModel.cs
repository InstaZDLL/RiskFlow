using System;
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
    public async Task<Analysis> CreateAnalysisAsync(string name, string modelKey,
        string? author = null, string? organization = null, string? projectDescription = null)
    {
        var nextOrder = Analyses.Count == 0 ? 0 : Analyses.Max(a => a.SortOrder) + 1;

        var analysis = new Analysis
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Analyse {Analyses.Count + 1}" : name.Trim(),
            ModelKey = RiskMatrixModels.Get(modelKey).Key,
            Author = Normalize(author),
            Organization = Normalize(organization),
            ProjectDescription = Normalize(projectDescription),
            SortOrder = nextOrder,
        };

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Analyses.Add(analysis);
        await db.SaveChangesAsync();

        Analyses.Add(analysis);
        SelectedAnalysis = analysis;
        return analysis;
    }

    /// <summary>Met à jour le nom et les informations de rapport d'une analyse existante.</summary>
    public async Task UpdateAnalysisAsync(Analysis analysis, string name,
        string? author, string? organization, string? projectDescription)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var entity = await db.Analyses.FindAsync(analysis.Id);
        if (entity is null)
            return;

        entity.Name = string.IsNullOrWhiteSpace(name) ? entity.Name : name.Trim();
        entity.Author = Normalize(author);
        entity.Organization = Normalize(organization);
        entity.ProjectDescription = Normalize(projectDescription);
        await db.SaveChangesAsync();

        // Répercute sur l'instance en mémoire (Name notifie la barre latérale).
        analysis.Name = entity.Name;
        analysis.Author = entity.Author;
        analysis.Organization = entity.Organization;
        analysis.ProjectDescription = entity.ProjectDescription;

        if (ReferenceEquals(SelectedAnalysis, analysis))
            risks.AnalysisName = analysis.Name;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Importe une analyse depuis un DTO : crée une nouvelle analyse + ses risques.</summary>
    public async Task ImportAnalysisAsync(Services.AnalysisExportDto dto)
    {
        var model = RiskMatrixModels.Get(dto.ModelKey);
        var maxSev = model.SeverityCount - 1;
        var maxLik = model.LikelihoodCount - 1;
        var nextOrder = Analyses.Count == 0 ? 0 : Analyses.Max(a => a.SortOrder) + 1;

        var analysis = new Analysis
        {
            Name = string.IsNullOrWhiteSpace(dto.Name) ? $"Analyse {Analyses.Count + 1}" : dto.Name.Trim(),
            ModelKey = model.Key,
            Author = Normalize(dto.Author),
            Organization = Normalize(dto.Organization),
            ProjectDescription = Normalize(dto.ProjectDescription),
            SortOrder = nextOrder,
        };

        var order = 0;
        foreach (var r in dto.Risks.OrderBy(r => r.SortOrder).ThenBy(r => r.RiskNumber))
        {
            analysis.Risks.Add(new Risk
            {
                RiskNumber = r.RiskNumber <= 0 ? order + 1 : r.RiskNumber,
                Title = r.Title,
                Category = string.IsNullOrWhiteSpace(r.Category) ? "Fonctionnel" : r.Category,
                Description = r.Description,
                BeforeSeverityIndex = Math.Clamp(r.BeforeSeverityIndex, 0, maxSev),
                BeforeLikelihoodIndex = Math.Clamp(r.BeforeLikelihoodIndex, 0, maxLik),
                MitigationStrategy = r.MitigationStrategy,
                AfterSeverityIndex = Math.Clamp(r.AfterSeverityIndex, 0, maxSev),
                AfterLikelihoodIndex = Math.Clamp(r.AfterLikelihoodIndex, 0, maxLik),
                CanContinue = r.CanContinue,
                SortOrder = order++,
            });
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        db.Analyses.Add(analysis);
        await db.SaveChangesAsync();

        Analyses.Add(analysis);
        SelectedAnalysis = analysis;
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
