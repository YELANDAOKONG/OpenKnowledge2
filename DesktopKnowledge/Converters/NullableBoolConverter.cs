using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class NullableBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? nullableBoolValue = value as bool?;
        string? paramString = parameter as string;
        if (nullableBoolValue != null && paramString != null)
        {
            bool paramValue = bool.Parse(paramString);
            return nullableBoolValue.HasValue && nullableBoolValue.Value == paramValue;
        }
        
        return false;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool)
        {
            bool boolValue = (bool)value;
            string? paramString = parameter as string;
            
            if (boolValue && paramString != null)
            {
                return bool.Parse(paramString);
            }
        }
        
        return null;
    }
}