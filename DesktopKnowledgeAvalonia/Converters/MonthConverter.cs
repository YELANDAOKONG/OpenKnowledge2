using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledgeAvalonia.Converters;

public class MonthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int month)
        {
            if (parameter == null) return null;
            // Find the month object with the corresponding number
            foreach (var item in (System.Collections.IEnumerable)parameter)
            {
                if (item is ViewModels.MonthOption monthOption && monthOption.Number == month)
                {
                    return monthOption;
                }
            }
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewModels.MonthOption month)
        {
            return month.Number;
        }
        return 1;
    }
}