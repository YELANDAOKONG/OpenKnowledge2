using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;
using DesktopKnowledgeAvalonia.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopKnowledgeAvalonia;

public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Set up dependency injection
        SetupExceptionHandling();
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        var localizationService = GetService<LocalizationService>();
        localizationService.LoadSavedLanguage();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            {
                // Check if we need to initialize the AI API settings
                var configService = GetService<ConfigureService>();
                bool needsInitialization = string.IsNullOrEmpty(configService.SystemConfig.OpenAiApiUrl) || 
                                           string.IsNullOrEmpty(configService.SystemConfig.OpenAiApiKey) || 
                                           string.IsNullOrEmpty(configService.SystemConfig.OpenAiModel);

                configService.AppStatistics.ApplicationStartCount++;
                _ = configService.SaveChangesAsync();
                if (needsInitialization)
                {
                    // Show the initialization window
                    var initWindow = new InitializationWindow();
                    desktop.MainWindow = initWindow; // Make it the main window initially
            
                    // Create the actual main window but don't show it yet
                    var mainModel = new MainWindowViewModel();
                    mainModel.IsWindowsVisible = false;
                    var mainWindow = new MainWindow(mainModel);
            
                    // When initialization completes, switch to the main window
                    initWindow.Closed += (s, e) => 
                    {
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                    };
                }
                else
                {
                    // No initialization needed, show main window directly
                    desktop.MainWindow = new MainWindow(new MainWindowViewModel());
                }
            }
            // desktop.MainWindow = new MainWindow(new MainWindowViewModel());
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices(ServiceCollection services)
    {
        // Register services
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<ConfigureService>();
        services.AddSingleton<ThemeService>();
        
        // Register view models
        services.AddTransient<MainWindowViewModel>();
    }
    
    private void SetupExceptionHandling()
    {
        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (sender, args) =>
        {
            args.Handled = true;
            
            var errorWindow = new FatalErrorWindow(args.Exception);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // errorWindow.ShowDialog(desktop.MainWindow);
                errorWindow.Show();
            }
        };
        
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            if (exception != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var errorWindow = new FatalErrorWindow(exception);
                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        errorWindow.ShowDialog(desktop.MainWindow!);
                    }
                    else
                    {
                        errorWindow.Show();
                    }
                });
            }
        };
    }



    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    public static T GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }
}