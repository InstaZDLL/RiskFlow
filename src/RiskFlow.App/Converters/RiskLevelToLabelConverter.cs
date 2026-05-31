using System;
using Microsoft.UI.Xaml.Data;
using RiskFlow.Core.Risks;
using RiskFlow.Services;

namespace RiskFlow.Converters;

/// <summary>Convertit un <see cref="RiskLevel"/> en libellé localisé (Bas/Moyen/Élevé/Critique…).</summary>
public class RiskLevelToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is RiskLevel level ? RiskText.Level(level) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
