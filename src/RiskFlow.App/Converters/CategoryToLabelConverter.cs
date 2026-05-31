using System;
using Microsoft.UI.Xaml.Data;
using RiskFlow.Services;

namespace RiskFlow.Converters;

/// <summary>
/// Traduit le nom d'une catégorie <b>par défaut</b> (mapping valeur stockée → clé de ressource).
/// Les catégories créées/renommées par l'utilisateur sont affichées telles quelles.
/// </summary>
public class CategoryToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => RiskText.Category(value as string ?? string.Empty);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
