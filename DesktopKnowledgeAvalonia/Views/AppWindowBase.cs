using System;
using Avalonia.Controls;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public class AppWindowBase : Window
{
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    
        var themeService = App.GetService<ThemeService>();
        themeService.ApplyTransparencyToWindow(this);
        themeService.ApplyThemeSettingsAsync().Wait();
        // Subscribe to theme changes
        this.ActualThemeVariantChanged += OnWindowThemeVariantChanged;
    }
    
    private void OnWindowThemeVariantChanged(object? sender, EventArgs e)
    {
        var themeService = App.GetService<ThemeService>();
        themeService.ApplyTransparencyToWindow(this);
    }

    public ViewModelBase? GetViewModel()
    {
        return DataContext as ViewModelBase;
    }

    public void SetViewModel(ViewModelBase viewModel)
    {
        DataContext = viewModel;
    }
}