using System;
using Avalonia.Controls;
using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.Views;

public class AppWindowBase : Window
{
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        var themeService = App.GetService<ThemeService>();
        themeService.ApplyTransparencyToWindow(this);
        themeService.ApplyThemeSettingsAsync().Wait();
    }
}