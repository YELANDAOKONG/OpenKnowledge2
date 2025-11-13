namespace DesktopKnowledge.Converters;

using Avalonia.Data.Converters;
using System;
using System.Globalization;

public class BoolToExpandCollapseIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isExpanded = (bool)value;
        return isExpanded
            ? "M7.41 15.41L12 10.83l4.59 4.58L18 14l-6-6-6 6z" // Up arrow
            : "M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z"; // Down arrow
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}