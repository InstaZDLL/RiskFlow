using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace RiskFlow.Core.Risks;

/// <summary>
/// Une analyse de risques : un registre de risques rattaché à un modèle de matrice.
/// L'application en gère plusieurs et permet de basculer de l'une à l'autre.
/// </summary>
public class Analysis : INotifyPropertyChanged
{
    private string _name = string.Empty;

    public int Id { get; set; }

    /// <summary>Nom affiché (notifie pour rafraîchir la barre latérale au renommage).</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Clé du modèle de matrice utilisé (« 3x4 », « 4x4 », « 5x5 »).</summary>
    public string ModelKey { get; set; } = RiskMatrixModels.Default.Key;

    // --- Informations de rapport (en-tête / page de garde du PDF), facultatives ---
    public string? Author { get; set; }
    public string? Organization { get; set; }
    public string? ProjectDescription { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Risques rattachés à cette analyse.</summary>
    public List<Risk> Risks { get; set; } = [];

    /// <summary>Modèle de matrice résolu à partir de <see cref="ModelKey"/>.</summary>
    [NotMapped]
    public RiskMatrixModel Model => RiskMatrixModels.Get(ModelKey);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
