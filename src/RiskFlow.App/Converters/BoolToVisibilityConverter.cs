using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RiskFlow.Converters;

/// <summary>Convertit un booléen en <see cref="Visibility"/> (true → Visible).</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility.Visible;
}
