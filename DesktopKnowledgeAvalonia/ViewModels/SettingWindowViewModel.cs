using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Models;
using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class SettingWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    private readonly ThemeService _themeService;

    [ObservableProperty]
    private ObservableCollection<SettingCategory> _categories = new();

    [ObservableProperty]
    private SettingCategory? _selectedCategory;

    public SettingWindowViewModel()
    {
        _configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _themeService = App.GetService<ThemeService>();

        InitializeCategories();
    }

    private void InitializeCategories()
    {
        // General Settings
        var general = new SettingCategory(
            _localizationService["settings.category.general"],
            "M19.43 12.98c.04-.32.07-.64.07-.98s-.03-.66-.07-.98l2.11-1.65c.19-.15.24-.42.12-.64l-2-3.46c-.12-.22-.39-.3-.61-.22l-2.49 1c-.52-.4-1.08-.73-1.69-.98l-.38-2.65C14.46 2.18 14.25 2 14 2h-4c-.25 0-.46.18-.49.42l-.38 2.65c-.61.25-1.17.59-1.69.98l-2.49-1c-.23-.09-.49 0-.61.22l-2 3.46c-.13.22-.07.49.12.64l2.11 1.65c-.04.32-.07.65-.07.98s.03.66.07.98l-2.11 1.65c-.19.15-.24.42-.12.64l2 3.46c.12.22.39.3.61.22l2.49-1c.52.4 1.08.73 1.69.98l.38 2.65c.03.24.24.42.49.42h4c.25 0 .46-.18.49-.42l.38-2.65c.61-.25 1.17-.59 1.69-.98l2.49 1c.23.09.49 0 .61-.22l2-3.46c.12-.22.07-.49-.12-.64l-2.11-1.65zM12 15.5c-1.93 0-3.5-1.57-3.5-3.5s1.57-3.5 3.5-3.5 3.5 1.57 3.5 3.5-1.57 3.5-3.5 3.5z",
            new GeneralSettingsViewModel(_configService, _localizationService));

        // AI Settings - Add this new category
        var ai = new SettingCategory(
            _localizationService["settings.category.ai"],
            "M21 11.5v-1c0-.8-.7-1.5-1.5-1.5H16v-2c0-.8-.7-1.5-1.5-1.5H9.5C8.7 5.5 8 6.2 8 7v2H4.5c-.8 0-1.5.7-1.5 1.5v1C2.2 11.5 1.5 12.2 1.5 13v9c0 .8.7 1.5 1.5 1.5h18c.8 0 1.5-.7 1.5-1.5v-9c0-.8-.7-1.5-1.5-1.5zM9 7.5h6v2H9v-2zm10 14H5v-9h14v9zm-9-7.5c0-1.1.9-2 2-2s2 .9 2 2c0 1.1-.9 2-2 2s-2-.9-2-2z",
            new AISettingsViewModel(_configService, _localizationService));

        // Appearance Settings
        var appearance = new SettingCategory(
            _localizationService["settings.category.appearance"],
            "M12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9 9-4.03 9-9-4.03-9-9-9zm0 16c-3.86 0-7-3.14-7-7s3.14-7 7-7 7 3.14 7 7-3.14 7-7 7zm-3-9h6v2H9z",
            new AppearanceSettingsViewModel(_configService, _themeService, _localizationService));

        // Language Settings
        var language = new SettingCategory(
            _localizationService["settings.category.language"],
            "M12.87 15.07l-2.54-2.51.03-.03c1.74-1.94 2.98-4.17 3.71-6.53H17V4h-7V2H8v2H1v2h11.17C11.5 7.92 10.44 9.75 9 11.35 8.07 10.32 7.3 9.19 6.69 8h-2c.73 1.63 1.73 3.17 2.98 4.56l-5.09 5.02L4 19l5-5 3.11 3.11.76-2.04zM18.5 10h-2L12 22h2l1.12-3h4.75L21 22h2l-4.5-12zm-2.62 7l1.62-4.33L19.12 17h-3.24z",
            new LanguageSettingsViewModel(_configService, _localizationService));

        Categories.Add(general);
        Categories.Add(ai); // Add the AI category
        Categories.Add(appearance);
        Categories.Add(language);

        SelectedCategory = general;
    }


    [RelayCommand]
    public async Task SaveAsync()
    {
        // Each settings view has its own Save method
        if (SelectedCategory?.Content is SettingsViewModelBase settingsView)
        {
            await settingsView.SaveAsync();
        }

        await _configService.SaveChangesAsync();
    }
}

// Base class for all settings views
public abstract class SettingsViewModelBase : ViewModelBase
{
    public abstract Task SaveAsync();
}

// General Settings View - contains username settings
public partial class GeneralSettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;

    [ObservableProperty]
    private string _userName;

    public GeneralSettingsViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _userName = _configService.AppConfig.UserName;
    }

    public override async Task SaveAsync()
    {
        _configService.AppConfig.UserName = UserName;
        await Task.CompletedTask;
    }
}

// Appearance Settings View - contains theme and transparency settings
public partial class AppearanceSettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly ThemeService _themeService;
    private readonly LocalizationService _localizationService;

    public string[] ThemeOptions { get; }
    
    [ObservableProperty]
    private string _selectedTheme;

    public string[] TransparencyOptions { get; }
    
    [ObservableProperty]
    private string _selectedTransparency;

    public AppearanceSettingsViewModel(ConfigureService configService, ThemeService themeService, LocalizationService localizationService)
    {
        _configService = configService;
        _themeService = themeService;
        _localizationService = localizationService;
        // Use localized theme options
        ThemeOptions = new[]
        {
            _localizationService["settings.theme.light"],
            _localizationService["settings.theme.dark"],
            _localizationService["settings.theme.system"]
        };
        TransparencyOptions = new[]
        {
            _localizationService["settings.transparency.auto"],
            _localizationService["settings.transparency.full"],
            _localizationService["settings.transparency.light"],
            _localizationService["settings.transparency.dark"]
        };
        _selectedTheme = _configService.AppConfig.ThemeVariant == null ? 
            ThemeOptions[2] : // System
            _configService.AppConfig.ThemeVariant == "Light" ? 
                ThemeOptions[0] : // Light
                ThemeOptions[1]; // Dark
        _selectedTransparency = _configService.AppConfig.TransparencyMode switch
        {
            TransparencyMode.Auto => TransparencyOptions[0],
            TransparencyMode.FullTransparency => TransparencyOptions[1],
            TransparencyMode.LightBackground => TransparencyOptions[2],
            TransparencyMode.DarkBackground => TransparencyOptions[3],
            _ => TransparencyOptions[0]
        };
    }

    public override async Task SaveAsync()
    {
        // Convert localized theme name back to internal value
        string? themeVariant = null;
        if (SelectedTheme == ThemeOptions[0]) // Light
            themeVariant = "Light";
        else if (SelectedTheme == ThemeOptions[1]) // Dark
            themeVariant = "Dark";
        // else leave as null for System
        _configService.AppConfig.ThemeVariant = themeVariant;
    
        _configService.AppConfig.TransparencyMode = SelectedTransparency switch
        {
            var t when t == TransparencyOptions[0] => TransparencyMode.Auto,
            var t when t == TransparencyOptions[1] => TransparencyMode.FullTransparency,
            var t when t == TransparencyOptions[2] => TransparencyMode.LightBackground,
            var t when t == TransparencyOptions[3] => TransparencyMode.DarkBackground,
            _ => TransparencyMode.Auto
        };
        _themeService.ApplyThemeSettingsAsync().Wait();
        await Task.CompletedTask;
    }
}

// Language Settings View - language selection
public partial class LanguageSettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;

    public string[] AvailableLanguages { get; }
    
    [ObservableProperty]
    private string _selectedLanguage;

    public LanguageSettingsViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;

        var languages = new List<string>();
        foreach (var key in localizationService.AvailableLanguages)
        {
            languages.Add(key);
        }
        AvailableLanguages = languages.ToArray();
        
        _selectedLanguage = _configService.AppConfig.PreferredLanguage ?? localizationService.CurrentLanguage;
    }

    public override async Task SaveAsync()
    {
        _configService.AppConfig.PreferredLanguage = SelectedLanguage;
        _localizationService.CurrentLanguage = SelectedLanguage;
        await _localizationService.SaveLanguagePreference();
    }
}

public class SettingCategory
{
    public string DisplayName { get; }
    public string IconPath { get; }
    public object Content { get; }

    public SettingCategory(string displayName, string iconPath, object content)
    {
        DisplayName = displayName;
        IconPath = iconPath;
        Content = content;
    }
}

// Add this class to the SettingWindowViewModel.cs file
public partial class AISettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private string _apiUrl = "";
    
    [ObservableProperty]
    private string _apiKey = "";
    
    [ObservableProperty]
    private string _model = "";
    
    [ObservableProperty]
    private double _temperature = 0.7;
    
    [ObservableProperty]
    private bool _isTestingConnection;
    
    [ObservableProperty]
    private bool _isConnectionTested;
    
    [ObservableProperty]
    private string _connectionStatus = "";
    
    [ObservableProperty]
    private Avalonia.Media.IBrush _connectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#569AFF"));
    
    [ObservableProperty]
    private bool _isApiKeyVisible = false;

    public char? ApiKeyPasswordChar => IsApiKeyVisible ? null : '•';

    public string ApiKeyVisibilityIcon => IsApiKeyVisible 
        ? "M12 6c-4.5 0-8 3.5-8 8s3.5 8 8 8 8-3.5 8-8-3.5-8-8-8zm0 14c-3.3 0-6-2.7-6-6s2.7-6 6-6 6 2.7 6 6-2.7 6-6 6zm0-10c-2.2 0-4 1.8-4 4s1.8 4 4 4 4-1.8 4-4-1.8-4-4-4zm0 6c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2z"
        : "M12 6c-4.5 0-8 3.5-8 8s3.5 8 8 8 8-3.5 8-8-3.5-8-8-8zm0 14c-3.3 0-6-2.7-6-6s2.7-6 6-6 6 2.7 6 6-2.7 6-6 6zm-1-7.5h2v2h-2v-2zm0-6h2v5h-2v-5z";

    public string TemperatureFormatted => Temperature.ToString("F2");
    
    public AISettingsViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        
        // Load existing values if available
        ApiUrl = _configService.SystemConfig.OpenAiApiUrl ?? "https://api.deepseek.com/v1";
        ApiKey = _configService.SystemConfig.OpenAiApiKey ?? "";
        Model = _configService.SystemConfig.OpenAiModel ?? "deepseek-chat";
        Temperature = _configService.SystemConfig.OpenAiModelTemperature ?? 0.7;
    }

    [RelayCommand]
    private void ToggleApiKeyVisibility()
    {
        IsApiKeyVisible = !IsApiKeyVisible;
        OnPropertyChanged(nameof(ApiKeyPasswordChar));
        OnPropertyChanged(nameof(ApiKeyVisibilityIcon));
    }

    public override async Task SaveAsync()
    {
        _configService.SystemConfig.OpenAiApiUrl = ApiUrl;
        _configService.SystemConfig.OpenAiApiKey = ApiKey;
        _configService.SystemConfig.OpenAiModel = Model;
        _configService.SystemConfig.OpenAiModelTemperature = Temperature;
        
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        // Validate fields first
        if (string.IsNullOrWhiteSpace(ApiUrl) || 
            string.IsNullOrWhiteSpace(ApiKey) || 
            string.IsNullOrWhiteSpace(Model))
        {
            ConnectionStatus = _localizationService["settings.ai.test.fill.all.fields"];
            ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E74C3C"));
            IsConnectionTested = true;
            return;
        }
    
        IsTestingConnection = true;
    
        try
        {
            // Create a temporary SystemConfig with current values
            var tempConfig = new LibraryOpenKnowledge.Models.SystemConfig
            {
                OpenAiApiUrl = ApiUrl,
                OpenAiApiKey = ApiKey,
                OpenAiModel = Model,
                OpenAiModelTemperature = Temperature
            };
        
            // Create OpenAI client using the SDK
            var client = LibraryOpenKnowledge.Tools.AiTools.CreateOpenAiClient(tempConfig);
        
            // Send a test message
            var response = await LibraryOpenKnowledge.Tools.AiTools.SendChatMessageAsync(
                client,
                tempConfig,
                "Hello, this is a test message.",
                throwExceptions: true
            );
        
            // If we get here, the connection was successful
            ConnectionStatus = _localizationService["settings.ai.test.success"];
            ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2ECC71"));
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"{_localizationService["settings.ai.test.error"]}: {ex.Message}";
            ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E74C3C"));
        }
        finally
        {
            IsTestingConnection = false;
            IsConnectionTested = true;
        }
    }
}
