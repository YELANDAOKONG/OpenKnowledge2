using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DesktopKnowledge.Converters;

public class CorrectColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCorrect)
        {
            return isCorrect 
                ? new SolidColorBrush(Color.Parse("#4CAF50")) // Green for correct
                : new SolidColorBrush(Color.Parse("#F44336")); // Red for incorrect
        }
        
        return new SolidColorBrush(Colors.Gray); // Default
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}