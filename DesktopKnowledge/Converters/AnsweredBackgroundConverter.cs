using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DesktopKnowledge.Converters;

public class AnsweredBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string[] answers && answers.Length > 0)
        {
            return new SolidColorBrush(Color.Parse("#22569AFF"));
        }
        return new SolidColorBrush(Colors.Transparent);
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}