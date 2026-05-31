using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using RiskFlow.Services;

namespace RiskFlow.Converters;

/// <summary>« Le projet peut continuer » → texte Oui/Non (localisé).</summary>
public class CanContinueToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => LanguageManager.Get(value is true ? "Detail_Yes" : "Detail_No");

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>« Le projet peut continuer » → couleur de fond (vert si oui, rouge si non).</summary>
public class CanContinueToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Yes = new(Color.FromArgb(255, 16, 185, 129));  // emerald-500
    private static readonly SolidColorBrush No = new(Color.FromArgb(255, 220, 38, 38));     // red-600

    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? Yes : No;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
