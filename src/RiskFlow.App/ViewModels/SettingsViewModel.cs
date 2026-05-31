using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RiskFlow.Core.Risks;
using RiskFlow.Data;
using RiskFlow.Services;

namespace RiskFlow.ViewModels;

/// <summary>Pilote la page Settings : apparence, affichage matrice, rapport, catégories.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly IDbContextFactory<RiskFlowDbContext> _dbFactory;
    private readonly RisksViewModel _risks;
    private bool _loading;

    public SettingsViewModel(SettingsService settings, IDbContextFactory<RiskFlowDbContext> dbFactory,
        RisksViewModel risks)
    {
        _settings = settings;
        _dbFactory = dbFactory;
        _risks = risks;

        var s = settings.Current;
        _loading = true;
        ThemeIndex = (int)s.Theme;
        LanguageIndex = (int)s.Language;
        MatrixPlacementIndex = (int)s.MatrixPlacement;
        MatrixCellContentIndex = (int)s.MatrixCellContent;
        MatrixEvalIndex = (int)s.MatrixDefaultEvaluation;
        ReportAuthor = s.ReportAuthor;
        ReportOrganization = s.ReportOrganization;
        _loading = false;
    }

    // ----- Apparence & matrice (index liés à des RadioButtons) -----
    [ObservableProperty] public partial int ThemeIndex { get; set; }
    [ObservableProperty] public partial int LanguageIndex { get; set; }
    [ObservableProperty] public partial int MatrixPlacementIndex { get; set; }
    [ObservableProperty] public partial int MatrixCellContentIndex { get; set; }
    [ObservableProperty] public partial int MatrixEvalIndex { get; set; }

    // ----- Rapport -----
    [ObservableProperty] public partial string ReportAuthor { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReportOrganization { get; set; } = string.Empty;

    /// <summary>Version de l'application (« Version 1.0.0 »).</summary>
    public string AppVersion { get; } =
        $"Version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}";

    // ----- Catégories -----
    public ObservableCollection<CategoryRowViewModel> Categories { get; } = [];

    [ObservableProperty] public partial string NewCategoryName { get; set; } = string.Empty;

    partial void OnThemeIndexChanged(int value) => Persist(s => s.Theme = (ThemeMode)value);

    // La langue est appliquée au prochain démarrage (un toast invite à redémarrer).
    partial void OnLanguageIndexChanged(int value) => Persist(s => s.Language = (AppLanguage)value);

    partial void OnMatrixPlacementIndexChanged(int value) => Persist(s => s.MatrixPlacement = (MatrixPlacement)value);
    partial void OnMatrixCellContentIndexChanged(int value) => Persist(s => s.MatrixCellContent = (MatrixCellContent)value);
    partial void OnMatrixEvalIndexChanged(int value) => Persist(s => s.MatrixDefaultEvaluation = (MatrixEvaluation)value);
    partial void OnReportAuthorChanged(string value) => Persist(s => s.ReportAuthor = value);
    partial void OnReportOrganizationChanged(string value) => Persist(s => s.ReportOrganization = value);

    private void Persist(System.Action<AppSettings> apply)
    {
        if (_loading)
            return;
        apply(_settings.Current);
        _settings.Save();
    }

    /// <summary>(Re)charge la liste des catégories depuis la base.</summary>
    public async Task LoadCategoriesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var items = await db.RiskCategories.OrderBy(c => c.SortOrder).ToListAsync();

        Categories.Clear();
        foreach (var c in items)
            Categories.Add(new CategoryRowViewModel(c.Id, c.Name));
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        var name = NewCategoryName.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        await using var db = await _dbFactory.CreateDbContextAsync();
        if (await db.RiskCategories.AnyAsync(c => c.Name == name))
            return; // doublon : ignoré

        var order = await db.RiskCategories.AnyAsync()
            ? await db.RiskCategories.MaxAsync(c => c.SortOrder) + 1
            : 0;

        var category = new RiskCategory { Name = name, SortOrder = order };
        db.RiskCategories.Add(category);
        await db.SaveChangesAsync();

        Categories.Add(new CategoryRowViewModel(category.Id, category.Name));
        NewCategoryName = string.Empty;
        await _risks.ReloadCategoriesAsync();
    }

    [RelayCommand]
    private async Task RenameCategoryAsync(CategoryRowViewModel? row)
    {
        if (row is null)
            return;

        var name = row.Name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await LoadCategoriesAsync(); // revert
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.RiskCategories.FindAsync(row.Id);
        if (entity is null)
            return;

        if (await db.RiskCategories.AnyAsync(c => c.Name == name && c.Id != row.Id))
        {
            await LoadCategoriesAsync(); // doublon : revert
            return;
        }

        entity.Name = name;
        await db.SaveChangesAsync();
        await _risks.ReloadCategoriesAsync();
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryRowViewModel? row)
    {
        if (row is null)
            return;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.RiskCategories.FindAsync(row.Id);
        if (entity is not null)
        {
            db.RiskCategories.Remove(entity);
            await db.SaveChangesAsync();
        }

        Categories.Remove(row);
        await _risks.ReloadCategoriesAsync();
    }
}
