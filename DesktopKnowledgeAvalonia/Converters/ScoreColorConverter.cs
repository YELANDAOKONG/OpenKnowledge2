using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DesktopKnowledgeAvalonia.Converters;

public class ScoreColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            if (percentage < 60)
                return new SolidColorBrush(Color.Parse("#FF4D4D")); // Red for fail
            else if (percentage < 80)
                return new SolidColorBrush(Color.Parse("#4285F4")); // Blue for pass
            else
                return new SolidColorBrush(Color.Parse("#4CAF50")); // Green for excellent
        }
        
        return new SolidColorBrush(Colors.Gray); // Default
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}