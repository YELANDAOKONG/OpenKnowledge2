using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledgeAvalonia.Converters
{
    public class BooleanMultiConverter : IMultiValueConverter
    {
        public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null)
                return false;
                
            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                    return false;
            }
            
            return true;
        }
    }
}