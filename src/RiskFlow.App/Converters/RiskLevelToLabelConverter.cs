using System;
using Microsoft.UI.Xaml.Data;
using RiskFlow.Core.Risks;

namespace RiskFlow.Converters;

/// <summary>Convertit un <see cref="RiskLevel"/> en libellé français (Bas / Moyen / Élevé / Critique).</summary>
public class RiskLevelToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is RiskLevel level ? level.ToFr() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
