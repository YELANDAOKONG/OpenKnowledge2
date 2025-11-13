using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class IndexPlusOneConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index + 1;
        }
        return 1;
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}