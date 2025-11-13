using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters
{
    public class BoolToExpandIconConverter : IValueConverter
    {
        public static readonly BoolToExpandIconConverter Instance = new BoolToExpandIconConverter();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                // Return down arrow if expanded, right arrow if collapsed
                return isExpanded
                    ? "M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z" // Down arrow
                    : "M10 17l5-5-5-5v10z"; // Right arrow
            }
            return "M10 17l5-5-5-5v10z"; // Default to right arrow
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}