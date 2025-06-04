using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Converters
{
    public class AnsweredBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string[] answers && answers.Length > 0)
            {
                return new SolidColorBrush(Color.Parse("#22569AFF"));
            }
            return new SolidColorBrush(Colors.Transparent);
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class AnsweredVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string[] answers && answers.Length > 0)
            {
                return true;
            }
            return false;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class NotNullVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class NotNullOrEmptyVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return false;
            
            if (value is Array array)
            {
                return array.Length > 0;
            }
            
            return true;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
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
    
    public class MultipleChoiceVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is QuestionTypes type)
            {
                return type == QuestionTypes.MultipleChoice;
            }
            return false;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class TextAnswerVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is QuestionTypes type)
            {
                return type == QuestionTypes.FillInTheBlank || 
                       type == QuestionTypes.Math || 
                       type == QuestionTypes.Essay || 
                       type == QuestionTypes.ShortAnswer || 
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
    
    public class JudgmentVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is QuestionTypes type)
            {
                return type == QuestionTypes.Judgment;
            }
            return false;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ReferenceMaterialImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ReferenceMaterialImage image)
            {
                switch (image.Type)
                {
                    case ReferenceMaterialImageTypes.Local:
                    case ReferenceMaterialImageTypes.Remote:
                        return image.Uri ?? "";
                        
                    case ReferenceMaterialImageTypes.Embedded:
                        if (image.Image != null)
                        {
                            // Convert byte array to image source
                            return new Avalonia.Media.Imaging.Bitmap(new System.IO.MemoryStream(image.Image));
                        }
                        break;
                }
            }
            return null!;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class IndexPlusOneConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1;
            }
            return 1;
        }
        
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
