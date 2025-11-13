using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class CollectionContainsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        
        if (parameter is IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item.ToString() == value.ToString())
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}