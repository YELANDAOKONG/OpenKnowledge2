using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Converters;

public class QuestionTypeOptionsVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QuestionTypes type)
        {
            return type == QuestionTypes.SingleChoice || type == QuestionTypes.MultipleChoice;
        }
        return false;
    }
        
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}