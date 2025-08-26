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
    
    public static ThemeVariantMode ToThemeVariantMode(ThemeVariant theme)
    {
        ThemeVariantMode variant;
        if (theme == ThemeVariant.Light)
        {
            variant = ThemeVariantMode.Light;
            
        }
        else if (theme == ThemeVariant.Dark)
        {
            variant = ThemeVariantMode.Dark;
        }
        else
        {
            variant = ThemeVariantMode.Default;
            
        }
        return variant;
    }
    
    public static ThemeVariantMode ToThemeVariantMode(string theme)
    {
        ThemeVariantMode variant;
        if (theme.Equals("Light"))
        {
            variant = ThemeVariantMode.Light;
            
        }
        else if (theme.Equals("Dark"))
        {
            variant = ThemeVariantMode.Dark;
        }
        else
        {
            variant = ThemeVariantMode.Default;
        }
        return variant;
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
            _configureService.AppConfig.ThemeVariant = ToThemeVariantMode(newTheme);
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

    public async Task ApplyThemeVariantAsync(ThemeVariantMode variant)
    {
        _configureService.AppConfig.ThemeVariant = variant;
        await _configureService.SaveChangesAsync();
        _ = ApplyThemeSettingsAsync();
    }
        
    private Task ApplyThemeVariantAsync()
    {
        if (Application.Current != null && _configureService.AppConfig.ThemeVariant != null)
        {
            // Convert string to ThemeVariant
            ThemeVariant? variant = null;
                
            if (_configureService.AppConfig.ThemeVariant == ThemeVariantMode.Light)
                variant = ThemeVariant.Light;
            else if (_configureService.AppConfig.ThemeVariant == ThemeVariantMode.Dark)
                variant = ThemeVariant.Dark;
            else if (_configureService.AppConfig.ThemeVariant == ThemeVariantMode.Default)
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
            var windows = new List<Window?> { desktop.MainWindow };
                
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
    
    private Avalonia.Media.Color ApplyAlphaToColor(Avalonia.Media.Color baseColor, double alpha)
    {
        // Ensure alpha is between 0 and 1
        var clampedAlpha = Math.Clamp(alpha, 0.0, 1.0);
        var alphaByte = (byte)(clampedAlpha * 255);
        return new Avalonia.Media.Color(alphaByte, baseColor.R, baseColor.G, baseColor.B);
    }
        
    public void ApplyTransparencyToWindow(Window window)
    {
        if (window == null) return;
    
        // Get the actual theme of the window (inherits from system if app theme is default)
        var actualTheme = window.ActualThemeVariant;
        var isLightTheme = actualTheme == ThemeVariant.Light;
    
        var transparencyMode = _configureService.AppConfig.TransparencyMode;
        var opacity = _configureService.AppConfig.BackgroundOpacity;
    
        var lightBaseColor = Avalonia.Media.Color.Parse("#FFFFFF"); // White
        var darkBaseColor = Avalonia.Media.Color.Parse("#101010");  // Very dark gray/black
    
        Avalonia.Media.Color finalColor;
    
        switch (transparencyMode)
        {
            case TransparencyMode.Auto:
                // Use light or dark background based on current theme
                var baseColor = isLightTheme ? lightBaseColor : darkBaseColor;
                finalColor = ApplyAlphaToColor(baseColor, opacity);
                window.Background = new Avalonia.Media.SolidColorBrush(finalColor);
                break;
    
            case TransparencyMode.FullTransparency:
                window.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                break;
        
            case TransparencyMode.LightBackground:
                finalColor = ApplyAlphaToColor(lightBaseColor, opacity);
                window.Background = new Avalonia.Media.SolidColorBrush(finalColor);
                break;
        
            case TransparencyMode.DarkBackground:
                finalColor = ApplyAlphaToColor(darkBaseColor, opacity);
                window.Background = new Avalonia.Media.SolidColorBrush(finalColor);
                break;
        }
    }

}