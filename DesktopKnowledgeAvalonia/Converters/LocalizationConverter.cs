using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledgeAvalonia.Converters;

public class LocalizationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key)
        {
            var localizationService = App.GetService<Services.LocalizationService>();
            return localizationService.Translate(key);
        }
        return value?.ToString() ?? string.Empty;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}