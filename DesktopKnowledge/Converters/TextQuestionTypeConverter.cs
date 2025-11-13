using System;
using System.Globalization;
using Avalonia.Data.Converters;
using OpenKnowledge.Models;

namespace DesktopKnowledge.Converters;

public class TextQuestionTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QuestionTypes type)
        {
            return type == QuestionTypes.FillInTheBlank || 
                   type == QuestionTypes.ShortAnswer || 
                   type == QuestionTypes.Essay || 
                   type == QuestionTypes.Math || 
                   type == QuestionTypes.Calculation || 
                   type == QuestionTypes.Complex || 
                   type == QuestionTypes.Other;
        }
        return false;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}