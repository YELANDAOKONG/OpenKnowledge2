using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
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
    
    private DateTime _applicationStartTime;
    private DateTime _lastStatisticsSaveTime;
    private DispatcherTimer? _statisticsTimer = null;
    
    public override void Initialize()
    {
        _applicationStartTime = DateTime.UtcNow;
        _lastStatisticsSaveTime = _applicationStartTime;
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
            {
                desktop.ShutdownRequested += OnShutdownRequested;
                desktop.Exit += OnExit;
                
                _statisticsTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(10) 
                };
                _statisticsTimer.Tick += SaveRuntimeStatistics;
                _statisticsTimer.Start();
            }
            
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            {
                // Check if we need to initialize the AI API settings
                var configService = GetService<ConfigureService>();
                bool needsInitialization = string.IsNullOrEmpty(configService.SystemConfig.OpenAiApiUrl) || 
                                           string.IsNullOrEmpty(configService.SystemConfig.OpenAiApiKey) || 
                                           string.IsNullOrEmpty(configService.SystemConfig.OpenAiModel);

                configService.AppStatistics.AddApplicationStartCount(configService);
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

    #region Statistics
    
    private void SaveRuntimeStatistics(object? sender, EventArgs e)
    {
        try
        {
            var now = DateTime.UtcNow;
            var intervalRunTime = (long)(now - _lastStatisticsSaveTime).TotalMilliseconds;
            
            var configService = GetService<ConfigureService>();
            if (configService.AppConfig.EnableStatistics)
            {
                configService.AppStatistics.AddApplicationRunTime(configService, intervalRunTime, true);
                _lastStatisticsSaveTime = now;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving runtime statistics: {ex.Message}");
        }
    }
    
    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        try
        {
            if (_statisticsTimer != null)
            {
                _statisticsTimer.Stop();
                _statisticsTimer.Tick -= SaveRuntimeStatistics;
                _statisticsTimer = null;
            }
            var finalIntervalRunTime = (long)(DateTime.UtcNow - _lastStatisticsSaveTime).TotalMilliseconds;
            
            var configService = GetService<ConfigureService>();
            if (configService.AppConfig.EnableStatistics && finalIntervalRunTime > 0)
            {
                configService.AppStatistics.AddApplicationRunTime(configService, finalIntervalRunTime, saveChanges: false);
                
                bool completed = Task.Run(async () => {
                    await configService.SaveChangesAsync();
                }).Wait(3000);
                
                if (!completed)
                {
                    Console.WriteLine("Warning: Failed to save statistics within timeout period.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving final application runtime statistics: {ex.Message}");
        }
    }
    
    private void OnExit(object? sender, EventArgs e)
    {
        try
        {
            if (_statisticsTimer != null)
            {
                _statisticsTimer.Stop();
                _statisticsTimer.Tick -= SaveRuntimeStatistics;
                _statisticsTimer = null;
            }
            
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _serviceProvider = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during application exit: {ex.Message}");
        }
    }
    
    #endregion

}