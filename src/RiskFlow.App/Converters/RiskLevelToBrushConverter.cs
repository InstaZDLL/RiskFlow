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
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not RiskLevel level)
            return new SolidColorBrush(Colors.Gray);

        var (a, r, g, b) = RiskPalette.Argb(level);
        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
