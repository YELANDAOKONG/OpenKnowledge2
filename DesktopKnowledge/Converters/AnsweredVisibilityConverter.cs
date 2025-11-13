using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class AnsweredVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string[] answers && answers.Length > 0)
        {
            return true;
        }
        return false;
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}