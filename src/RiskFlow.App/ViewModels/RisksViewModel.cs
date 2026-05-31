using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
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
    [NotifyCanExecuteChangedFor(nameof(MoveRiskUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveRiskDownCommand))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial RiskRowViewModel? SelectedRow { get; set; }

    /// <summary>Affiche les colonnes Description et Stratégie du tableau (masquées par défaut).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextColumnWidth))]
    [NotifyPropertyChangedFor(nameof(TextColumnVisibility))]
    public partial bool ShowDescriptions { get; set; }

    public GridLength TextColumnWidth => ShowDescriptions ? new GridLength(240) : new GridLength(0);
    public Visibility TextColumnVisibility => ShowDescriptions ? Visibility.Visible : Visibility.Collapsed;

    // ----- Cartes de synthèse (niveaux avant mitigation, comme TPI-Flow) -----
    [ObservableProperty] public partial int TotalRisks { get; set; }
    [ObservableProperty] public partial int CriticalCount { get; set; }
    [ObservableProperty] public partial int HighCount { get; set; }
    [ObservableProperty] public partial int BlockedCount { get; set; }

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
        foreach (var row in Rows)
            row.PropertyChanged -= OnRowChanged;
        Rows.Clear();
        SelectedRow = null;

        if (_analysis is null)
        {
            RecomputeStats();
            return;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var items = await db.Risks
            .Where(r => r.AnalysisId == _analysis.Id)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.RiskNumber)
            .ToListAsync();

        foreach (var risk in items)
        {
            var row = new RiskRowViewModel(risk, _model);
            row.PropertyChanged += OnRowChanged;
            Rows.Add(row);
        }

        RecomputeStats();
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(RiskRowViewModel.BeforeLevel) or nameof(RiskRowViewModel.CanContinue))
            RecomputeStats();
    }

    private void RecomputeStats()
    {
        TotalRisks = Rows.Count;
        CriticalCount = Rows.Count(r => r.BeforeLevel == RiskLevel.Extreme);
        HighCount = Rows.Count(r => r.BeforeLevel == RiskLevel.High);
        BlockedCount = Rows.Count(r => !r.CanContinue);
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
        row.PropertyChanged += OnRowChanged;
        Rows.Add(row);
        SelectedRow = row;
        RecomputeStats();
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

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private Task MoveRiskUpAsync()
    {
        var index = SelectedRow is null ? -1 : Rows.IndexOf(SelectedRow);
        if (index <= 0)
            return Task.CompletedTask;

        Rows.Move(index, index - 1);
        return RenumberAndPersistAsync();
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private Task MoveRiskDownAsync()
    {
        var index = SelectedRow is null ? -1 : Rows.IndexOf(SelectedRow);
        if (index < 0 || index >= Rows.Count - 1)
            return Task.CompletedTask;

        Rows.Move(index, index + 1);
        return RenumberAndPersistAsync();
    }

    private bool CanMoveUp() => SelectedRow is not null && Rows.IndexOf(SelectedRow) > 0;
    private bool CanMoveDown() => SelectedRow is not null && Rows.IndexOf(SelectedRow) < Rows.Count - 1;

    /// <summary>Réaffecte l'ordre et le numéro (1..n) selon la position, puis persiste.</summary>
    private async Task RenumberAndPersistAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        for (var i = 0; i < Rows.Count; i++)
        {
            Rows[i].SetOrder(i, i + 1);
            db.Risks.Update(Rows[i].Model);
        }
        await db.SaveChangesAsync();

        MoveRiskUpCommand.NotifyCanExecuteChanged();
        MoveRiskDownCommand.NotifyCanExecuteChanged();
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

        row.PropertyChanged -= OnRowChanged;
        Rows.Remove(row);
        RecomputeStats();
    }

    private bool CanDeleteRisk(RiskRowViewModel? row) => (row ?? SelectedRow) is not null;
}
