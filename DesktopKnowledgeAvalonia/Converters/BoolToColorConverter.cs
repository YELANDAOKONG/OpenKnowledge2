using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DesktopKnowledgeAvalonia.Converters;

public class BoolToColorConverter : IValueConverter
{
    public IBrush TrueValue { get; set; } = new SolidColorBrush(Colors.Blue);
    public IBrush FalseValue { get; set; } = new SolidColorBrush(Colors.Transparent);
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        return FalseValue;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}