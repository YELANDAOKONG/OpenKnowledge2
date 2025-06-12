using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesktopKnowledgeAvalonia.Models;

namespace DesktopKnowledgeAvalonia.Services;

public class ThemeService
{
    private readonly ConfigureService _configureService;
    
    private readonly LoggerService _logger;
        
    public ThemeService(ConfigureService configureService)
    {
        _configureService = configureService;
        _logger = App.GetLogger("ThemeService");
        _logger.Info("Initializing theme service...");
    }
        
    public async Task ApplyThemeSettingsAsync()
    {
        await ApplyThemeVariantAsync();
        ApplyTransparencyMode();
    }
        
    public async Task ToggleThemeVariantAsync()
    {
        if (Application.Current != null)
        {
            // Toggle between Light and Dark
            var newTheme = Application.Current.RequestedThemeVariant == ThemeVariant.Light 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
                
            Application.Current.RequestedThemeVariant = newTheme;
            _configureService.AppConfig.ThemeVariant = newTheme.ToString();
            await _configureService.SaveChangesAsync();
            ApplyTransparencyMode();
        }
    }
        
    public async Task SetTransparencyModeAsync(TransparencyMode mode)
    {
        _configureService.AppConfig.TransparencyMode = mode;
        await _configureService.SaveChangesAsync();
        ApplyTransparencyMode();
    }
        
    private Task ApplyThemeVariantAsync()
    {
        if (Application.Current != null && !string.IsNullOrEmpty(_configureService.AppConfig.ThemeVariant))
        {
            // Convert string to ThemeVariant
            ThemeVariant? variant = null;
                
            if (_configureService.AppConfig.ThemeVariant == "Light")
                variant = ThemeVariant.Light;
            else if (_configureService.AppConfig.ThemeVariant == "Dark")
                variant = ThemeVariant.Dark;
            else if (_configureService.AppConfig.ThemeVariant == "Default")
                variant = ThemeVariant.Default;
                
            if (variant != null)
                Application.Current.RequestedThemeVariant = variant;
        }

        return Task.CompletedTask;
    }
        
    private void ApplyTransparencyMode()
    {
        // Get all top-level windows using a safer approach
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            var windows = new List<Window> { desktop.MainWindow };
                
            // Add other windows if needed
            if (desktop.Windows != null)
            {
                foreach (var window in desktop.Windows)
                {
                    if (window != desktop.MainWindow && window is Window w)
                        windows.Add(w);
                }
            }
                
            foreach (var window in windows)
            {
                if (window != null)
                    ApplyTransparencyToWindow(window);
            }
        }
    }
        
    public void ApplyTransparencyToWindow(Window window)
    {
        if (window == null) return;
        
        var isLightTheme = Application.Current?.RequestedThemeVariant == ThemeVariant.Light;
        var transparencyMode = _configureService.AppConfig.TransparencyMode;
        
        switch (transparencyMode)
        {
            case TransparencyMode.Auto:
                // Use light or dark background based on current theme
                if (isLightTheme)
                    window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0FFFFFF"));
                else
                    window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0202020"));
                break;
            
            case TransparencyMode.FullTransparency:
                window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                break;
                
            case TransparencyMode.LightBackground:
                window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0FFFFFF"));
                break;
                
            case TransparencyMode.DarkBackground:
                window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0202020"));
                break;
        }
    }

}