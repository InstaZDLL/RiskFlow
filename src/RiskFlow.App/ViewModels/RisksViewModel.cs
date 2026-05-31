using System;
using System.Collections.Generic;
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
/// suppression et édition. Les niveaux de risque dépendent du modèle de matrice.
/// </summary>
public partial class RisksViewModel(IDbContextFactory<RiskFlowDbContext> dbFactory) : ObservableObject
{
    private Analysis? _analysis;
    private RiskMatrixModel _model = RiskMatrixModels.Default;

    public ObservableCollection<RiskRowViewModel> Rows { get; } = [];

    /// <summary>Catégories disponibles (partagées entre analyses).</summary>
    public ObservableCollection<string> Categories { get; } = [];

    /// <summary>Libellés de gravité du modèle courant (source des listes déroulantes).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> SeverityLevels { get; set; } = [];

    /// <summary>Libellés de probabilité du modèle courant.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> LikelihoodLevels { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteRiskCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveSelectedCommand))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial RiskRowViewModel? SelectedRow { get; set; }

    [ObservableProperty]
    public partial string AnalysisName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ModelName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddRiskCommand))]
    public partial bool HasAnalysis { get; set; }

    /// <summary>Un risque est sélectionné (le panneau de détail est affiché).</summary>
    public bool HasSelection => SelectedRow is not null;

    /// <summary>Modèle de matrice de l'analyse courante (axes + grille de niveaux).</summary>
    public RiskMatrixModel CurrentModel => _model;

    /// <summary>
    /// Synchronise les éventuelles éditions en cours dans les entités et renvoie les
    /// risques de l'analyse, dans l'ordre d'affichage, pour l'export PDF.
    /// </summary>
    public IReadOnlyList<Risk> SnapshotForExport()
    {
        foreach (var row in Rows)
            row.ApplyToModel();
        return Rows.Select(r => r.Model).ToList();
    }

    /// <summary>Affiche brièvement la confirmation « Sauvegardé ».</summary>
    [ObservableProperty]
    public partial bool SavedMessageVisible { get; set; }

    /// <summary>Fixe l'analyse affichée, recharge ses risques et les catégories.</summary>
    public async Task SetAnalysisAsync(Analysis? analysis)
    {
        _analysis = analysis;
        _model = analysis?.Model ?? RiskMatrixModels.Default;
        AnalysisName = analysis?.Name ?? string.Empty;
        ModelName = _model.Name;
        HasAnalysis = analysis is not null;
        SeverityLevels = _model.SeverityLevels;
        LikelihoodLevels = _model.LikelihoodLevels;

        await LoadCategoriesAsync();
        await LoadAsync();
    }

    /// <summary>Recharge les catégories (après modification dans les réglages).</summary>
    public Task ReloadCategoriesAsync() => LoadCategoriesAsync();

    private async Task LoadCategoriesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var names = await db.RiskCategories
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Name)
            .ToListAsync();

        Categories.Clear();
        foreach (var name in names)
            Categories.Add(name);
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
            Category = Categories.FirstOrDefault() ?? "Fonctionnel",
            SortOrder = nextOrder,
        };

        db.Risks.Add(risk);
        await db.SaveChangesAsync();

        var row = new RiskRowViewModel(risk, _model);
        Rows.Add(row);
        SelectedRow = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task SaveSelectedAsync()
    {
        var row = SelectedRow;
        if (row is null)
            return;

        row.ApplyToModel();

        await using (var db = await dbFactory.CreateDbContextAsync())
        {
            db.Risks.Update(row.Model);
            await db.SaveChangesAsync();
        }

        // Ferme le panneau et affiche brièvement la confirmation.
        SelectedRow = null;
        SavedMessageVisible = true;
        await Task.Delay(TimeSpan.FromSeconds(2));
        SavedMessageVisible = false;
    }

    /// <summary>Ferme le panneau de détail (désélectionne le risque courant).</summary>
    [RelayCommand]
    private void CloseDetail() => SelectedRow = null;

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
