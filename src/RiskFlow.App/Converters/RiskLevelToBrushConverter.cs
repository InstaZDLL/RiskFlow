using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using RiskFlow.Core.Risks;

namespace RiskFlow.Converters;

/// <summary>
/// Convertit un <see cref="RiskLevel"/> en couleur de fond.
/// Palette alignée sur getRiskLevelColor (TPI-Flow) : vert / ambre / orange / rouge.
/// </summary>
public class RiskLevelToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Low = new(Color.FromArgb(255, 16, 185, 129));     // emerald-500
    private static readonly SolidColorBrush Medium = new(Color.FromArgb(255, 251, 191, 36));  // amber-400
    private static readonly SolidColorBrush High = new(Color.FromArgb(255, 249, 115, 22));    // orange-500
    private static readonly SolidColorBrush Extreme = new(Color.FromArgb(255, 220, 38, 38));  // red-600

    public object Convert(object value, Type targetType, object parameter, string language) => value switch
    {
        RiskLevel.Low => Low,
        RiskLevel.Medium => Medium,
        RiskLevel.High => High,
        RiskLevel.Extreme => Extreme,
        _ => new SolidColorBrush(Colors.Gray),
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
