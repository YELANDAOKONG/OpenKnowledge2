using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class EnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        
        if (value is Enum enumValue && parameter is string strParam)
        {
            return enumValue.ToString() == strParam;
        }
        
        return false;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}