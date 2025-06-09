using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Controls;

namespace DesktopKnowledgeAvalonia.Converters;

public class PercentToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            percentage = Math.Max(0, percentage);
            return new GridLength(percentage, GridUnitType.Star);
        }
        return new GridLength(1, GridUnitType.Auto);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}