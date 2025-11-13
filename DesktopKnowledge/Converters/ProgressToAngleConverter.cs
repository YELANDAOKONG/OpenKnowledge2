using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class ProgressToAngleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            // Convert progress (0-100) to sweep angle (0-360)
            return 3.6 * progress;
        }
        
        return 0;
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}