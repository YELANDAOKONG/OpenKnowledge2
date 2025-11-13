using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DesktopKnowledge.Services;
using OpenKnowledge.Models;

namespace DesktopKnowledge.Converters;

public class QuestionTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QuestionTypes type)
        {
            var localizationService = App.GetService<LocalizationService>();
            
            string key = type switch
            {
                QuestionTypes.SingleChoice => "exam.question.type.single",
                QuestionTypes.MultipleChoice => "exam.question.type.multiple",
                QuestionTypes.Judgment => "exam.question.type.judgment",
                QuestionTypes.FillInTheBlank => "exam.question.type.fill",
                QuestionTypes.Math => "exam.question.type.math",
                QuestionTypes.Essay => "exam.question.type.essay",
                QuestionTypes.ShortAnswer => "exam.question.type.short",
                QuestionTypes.Calculation => "exam.question.type.calculation",
                QuestionTypes.Complex => "exam.question.type.complex",
                QuestionTypes.Other => "exam.question.type.other",
                _ => "exam.question.type.other"
            };
            
            return localizationService.Translate(key);
        }
        
        return "Unknown";
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}