using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace DesktopKnowledge.Converters;

public class WindowStateToHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowState state && parameter is string param)
        {
            bool isFullscreen = state == WindowState.FullScreen || state == WindowState.Maximized;

            switch (param)
            {
                case "Height":
                    return isFullscreen ? double.NaN : 300; // NaN means "Auto"
                case "MaxHeight":
                    return isFullscreen ? 800.0 : 500.0;
                default:
                    throw new ArgumentException($"Unknown parameter: {param}");
            }
        }
        
        return parameter == "Height" ? 300.0 : 500.0; // Default values
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}