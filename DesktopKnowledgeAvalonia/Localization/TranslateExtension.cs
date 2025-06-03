using Avalonia.Markup.Xaml;
using Avalonia.Data;
using System;
using Avalonia;

namespace DesktopKnowledgeAvalonia.Localization;

public class TranslateExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var localizationService = App.GetService<Services.LocalizationService>();
        if (localizationService == null)
            return Key;

        return new Binding
        {
            Source = localizationService,
            Path = $"[{Key}]",
        };
    }
}