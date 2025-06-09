using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace DesktopKnowledgeAvalonia.Converters;

public class PercentToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            return new GridLength(percent, GridUnitType.Star);
        }
        return new GridLength(0, GridUnitType.Star);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}