﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledgeAvalonia.Converters;

public class StringEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return parameter.ToString();
        }
        return null;
    }
}
