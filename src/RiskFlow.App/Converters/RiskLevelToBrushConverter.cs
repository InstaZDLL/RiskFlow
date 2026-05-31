using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using RiskFlow.Core.Risks;

namespace RiskFlow.Converters;

/// <summary>
/// Convertit un <see cref="RiskLevel"/> en couleur de fond.
/// Palette alignée sur getRiskLevelColor (TPI-Flow) : vert / ambre / orange / rouge.
/// Les brushes sont mis en cache (une instance par niveau, partageable entre éléments).
/// </summary>
public class RiskLevelToBrushConverter : IValueConverter
{
    private static readonly Dictionary<RiskLevel, SolidColorBrush> Cache = new();
    private static readonly SolidColorBrush Fallback = new(Colors.Gray);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not RiskLevel level)
            return Fallback;

        if (!Cache.TryGetValue(level, out var brush))
        {
            var (a, r, g, b) = RiskPalette.Argb(level);
            brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            Cache[level] = brush;
        }

        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
