using System;
using System.Globalization;
using Avalonia.Data.Converters;
using OpenKnowledge.Models;

namespace DesktopKnowledge.Converters;

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