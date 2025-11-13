using System;
using System.Globalization;
using Avalonia.Data.Converters;
using OpenKnowledge.Models;

namespace DesktopKnowledge.Converters;

public class SingleChoiceVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QuestionTypes type)
        {
            return type == QuestionTypes.SingleChoice;
        }
        return false;
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}