using System;
using Microsoft.UI.Xaml.Data;
using RiskFlow.Services;

namespace RiskFlow.Converters;

/// <summary>
/// Renvoie la chaîne traduite (ressources .resw via <see cref="LanguageManager"/>) dont la clé
/// est passée en <c>ConverterParameter</c> (ou via la valeur liée, ex. <c>Tag</c>).
/// </summary>
public class LocalizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => LanguageManager.Get((parameter as string) ?? (value as string) ?? string.Empty);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
