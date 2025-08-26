using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Models;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Tools;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class SettingWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    private readonly ThemeService _themeService;
    
    public event EventHandler? WindowCloseRequested;

    [ObservableProperty]
    private ObservableCollection<SettingCategory> _categories = new();

    [ObservableProperty]
    private SettingCategory? _selectedCategory;
    
    private readonly LoggerService _logger;

    public SettingWindowViewModel()
    {
        _configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _themeService = App.GetService<ThemeService>();
        _logger = App.GetWindowsLogger("SettingWindow");

        InitializeCategories();
    }

    private void InitializeCategories()
    {
        // General Settings
        var general = new SettingCategory(
            _localizationService["settings.category.general"],
            "M19.43 12.98c.04-.32.07-.64.07-.98s-.03-.66-.07-.98l2.11-1.65c.19-.15.24-.42.12-.64l-2-3.46c-.12-.22-.39-.3-.61-.22l-2.49 1c-.52-.4-1.08-.73-1.69-.98l-.38-2.65C14.46 2.18 14.25 2 14 2h-4c-.25 0-.46.18-.49.42l-.38 2.65c-.61.25-1.17.59-1.69.98l-2.49-1c-.23-.09-.49 0-.61.22l-2 3.46c-.13.22-.07.49.12.64l2.11 1.65c-.04.32-.07.65-.07.98s.03.66.07.98l-2.11 1.65c-.19.15-.24.42-.12.64l2 3.46c.12.22.39.3.61.22l2.49-1c.52.4 1.08.73 1.69.98l.38 2.65c.03.24.24.42.49.42h4c.25 0 .46-.18.49-.42l.38-2.65c.61-.25 1.17-.59 1.69-.98l2.49 1c.23.09.49 0 .61-.22l2-3.46c.12-.22.07-.49-.12-.64l-2.11-1.65zM12 15.5c-1.93 0-3.5-1.57-3.5-3.5s1.57-3.5 3.5-3.5 3.5 1.57 3.5 3.5-1.57 3.5-3.5 3.5z",
            new GeneralSettingsViewModel(_logger, _configService, _localizationService));

        // AI Settings - Add this new category
        var ai = new SettingCategory(
            _localizationService["settings.category.ai"],
            "M21 11.5v-1c0-.8-.7-1.5-1.5-1.5H16v-2c0-.8-.7-1.5-1.5-1.5H9.5C8.7 5.5 8 6.2 8 7v2H4.5c-.8 0-1.5.7-1.5 1.5v1C2.2 11.5 1.5 12.2 1.5 13v9c0 .8.7 1.5 1.5 1.5h18c.8 0 1.5-.7 1.5-1.5v-9c0-.8-.7-1.5-1.5-1.5zM9 7.5h6v2H9v-2zm10 14H5v-9h14v9zm-9-7.5c0-1.1.9-2 2-2s2 .9 2 2c0 1.1-.9 2-2 2s-2-.9-2-2z",
            new AISettingsViewModel(_logger, _configService, _localizationService));

        // Prompt Templates Settings
        var promptTemplates = new SettingCategory(
            _localizationService["settings.category.prompts"],
            "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-2 16H7v-2h10v2zm0-4H7v-2h10v2zm0-4H7V9h10v2zm0-4H7V5h10v2z",
            new PromptTemplatesViewModel(_logger, _configService, _localizationService));
        
        // Appearance Settings
        var appearance = new SettingCategory(
            _localizationService["settings.category.appearance"],
            "M12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9 9-4.03 9-9-4.03-9-9-9zm0 16c-3.86 0-7-3.14-7-7s3.14-7 7-7 7 3.14 7 7-3.14 7-7 7zm-3-9h6v2H9z",
            new AppearanceSettingsViewModel(_logger, _configService, _themeService, _localizationService));

        // Language Settings
        var language = new SettingCategory(
            _localizationService["settings.category.language"],
            "M12.87 15.07l-2.54-2.51.03-.03c1.74-1.94 2.98-4.17 3.71-6.53H17V4h-7V2H8v2H1v2h11.17C11.5 7.92 10.44 9.75 9 11.35 8.07 10.32 7.3 9.19 6.69 8h-2c.73 1.63 1.73 3.17 2.98 4.56l-5.09 5.02L4 19l5-5 3.11 3.11.76-2.04zM18.5 10h-2L12 22h2l1.12-3h4.75L21 22h2l-4.5-12zm-2.62 7l1.62-4.33L19.12 17h-3.24z",
            new LanguageSettingsViewModel(_logger, _configService, _localizationService));
        
        // Storage Settings
        var storage = new SettingCategory(
            _localizationService["settings.category.storage"],
            "M6 2c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6H6zm7 7V3.5L18.5 9H13z",
            new StorageSettingsViewModel(_logger, _configService, _localizationService));
        
        // About Settings
        var about = new SettingCategory(
            _localizationService["settings.category.about"],
            "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z", // Info icon path
            new AboutSettingsViewModel(_logger, _configService, _localizationService));

        Categories.Add(general);
        Categories.Add(ai); // Add the AI category
        Categories.Add(promptTemplates); // Add the prompt templates category
        Categories.Add(appearance);
        Categories.Add(language);
        Categories.Add(storage);
        Categories.Add(about);

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
    
    [RelayCommand]
    public async Task SaveExitAsync()
    {
        await SaveAsync();
        WindowCloseRequested?.Invoke(this, EventArgs.Empty);
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
    
    [ObservableProperty]
    private bool _enableStatistics;
    
    [ObservableProperty]
    private bool _randomizeWelcomeMessage;
    
    [ObservableProperty]
    private Bitmap? _avatarImage;
    
    [ObservableProperty]
    private string? _userInitials;
    
    
    [ObservableProperty]
    private bool _isAvatarMessageVisible;
    
    [ObservableProperty]
    private string _avatarMessage = "";
    
    [ObservableProperty]
    private Avalonia.Media.IBrush _avatarMessageBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#569AFF"));
    
    public List<string> LogLevels { get; } = new List<string> 
    { 
        "Trace", 
        "Debug", 
        "Information", 
        "Warning", 
        "Error", 
        "Critical", 
        "None" 
    };
    
    [ObservableProperty]
    private string _selectedLogLevel = "Information";
    
    private readonly LoggerService _logger;

    public GeneralSettingsViewModel(LoggerService logger, ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("GeneralSettings");
        _configService = configService;
        _localizationService = localizationService;
        _userName = _configService.AppConfig.UserName;
        _enableStatistics = _configService.AppConfig.EnableStatistics;
        _randomizeWelcomeMessage = _configService.AppConfig.RandomizeWelcomeMessage;
        _selectedLogLevel = _configService.AppConfig.LogLevel.ToString();
        
        UpdateUserInitials();
        LoadAvatarAsync().ConfigureAwait(false);
    }
    
    private void UpdateUserInitials()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            UserInitials = "?";
            return;
        }
        
        var parts = UserName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            UserInitials = "?";
        }
        else if (parts.Length == 1)
        {
            UserInitials = parts[0].Length > 0 ? parts[0][0].ToString().ToUpper() : "?";
        }
        else
        {
            UserInitials = (parts[0].Length > 0 ? parts[0][0].ToString() : "") + 
                          (parts[^1].Length > 0 ? parts[^1][0].ToString() : "");
            UserInitials = UserInitials.ToUpper();
        }
    }
    
    private async Task LoadAvatarAsync()
    {
        var avatarPath = _configService.AppConfig.AvatarFilePath;
        
        if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
        {
            _logger.Info($"Loading avatar from {avatarPath}");
            try
            {
                await using var stream = File.OpenRead(avatarPath);
                AvatarImage = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                ShowAvatarSizeError("settings.general.avatar.error.loading");
                _logger.Error($"Error loading avatar: {ex.Message}");
                _logger.Trace($"Error loading avatar: {ex.StackTrace}");
                AvatarImage = null;
                
                var configService = App.GetService<ConfigureService>();
                configService.AppConfig.AvatarFilePath = null;
                _ = configService.SaveChangesAsync();
            }
        }
        else
        {
            AvatarImage = null;
        }
    }
    
    // Method to display avatar notifications (will be called from code-behind)
    public void ShowAvatarSizeError(string? localizationKey = null)
    {
        var key = localizationKey ?? "settings.general.avatar.error.size";
        AvatarMessage = _localizationService[key];
        AvatarMessageBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E74C3C"));
        IsAvatarMessageVisible = true;
    }
    
    // Reset notification
    public void ResetAvatarMessage()
    {
        IsAvatarMessageVisible = false;
    }
    
    // Add a method to update avatar from code-behind
    public async Task UpdateAvatarAsync(string? avatarPath)
    {
        _configService.AppConfig.AvatarFilePath = avatarPath;
        await LoadAvatarAsync();
    }
    
    [RelayCommand]
    private async Task ChangeAvatarAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
            
        // Create storage provider
        var topLevel = desktop.MainWindow;
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider == null)
            return;
            
        // Set up file picker options
        var options = new FilePickerOpenOptions
        {
            Title = _localizationService["settings.general.avatar.change"],
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType(_localizationService["settings.general.avatar.file.types"])
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" },
                    MimeTypes = new[] { "image/jpeg", "image/png", "image/bmp" }
                }
            }
        };
        
        // Show file picker
        var files = await storageProvider.OpenFilePickerAsync(options);
        if (files.Count == 0)
            return;
            
        var file = files[0];
        
        try
        {
            // Check file size
            var fileInfo = await file.GetBasicPropertiesAsync();
            _logger.Info($"File path: {file.Path}");
            _logger.Info($"File size: {fileInfo.Size} Bytes");
            if (fileInfo.Size > 64 * 1024 * 1024) // 64MB limit
            {
                _logger.Info($"File size exceeds limit ( {fileInfo.Size} Bytes / 64MB)");
                // Show error message
                // In a real app, you'd show a dialog here
                Console.WriteLine(_localizationService["settings.general.avatar.error.size"]);
                return;
            }
            
            // Create avatar directory
            var avatarDir = ConfigureService.GetAvatarDirectory();
                
            // Generate target file path
            var extension = Path.GetExtension((string) file.Name);
            var avatarFileName = $"avatar_{Guid.NewGuid()}{extension}";
            var avatarPath = Path.Combine(avatarDir, avatarFileName);
            _logger.Info($"Use file name: {avatarFileName}");
            
            // Copy the file
            await using (var sourceStream = await file.OpenReadAsync())
            await using (var destinationStream = File.Create(avatarPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            _logger.Info($"Avatar copied to: {avatarPath}");
            
            // Save the new path and load the image
            _configService.AppConfig.AvatarFilePath = avatarPath;
            await LoadAvatarAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error handling avatar: {ex.Message}");
            _logger.Trace($"Error handling avatar: {ex.StackTrace}");
        }
    }
    
    [RelayCommand]
    private async Task RemoveAvatarAsync()
    {
        // Remove avatar path from config
        var oldPath = _configService.AppConfig.AvatarFilePath;
        _configService.AppConfig.AvatarFilePath = null;
        
        // Clear avatar image
        AvatarImage = null;
        
        // Delete the old avatar file if it exists
        if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath))
        {
            try
            {
                File.Delete(oldPath);
                _logger.Info($"Avatar file deleted: {oldPath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting avatar file: {ex.Message}");
                _logger.Trace($"Error deleting avatar file: {ex.StackTrace}");
            }
        }

        await ConfigureService.ClearAvatars(null);
    }

    public override async Task SaveAsync()
    {
        _configService.AppConfig.UserName = UserName;
        _configService.AppConfig.EnableStatistics = EnableStatistics;
        _configService.AppConfig.RandomizeWelcomeMessage = RandomizeWelcomeMessage;
        
        // Save log level
        if (Enum.TryParse<Microsoft.Extensions.Logging.LogLevel>(SelectedLogLevel, out var logLevel))
        {
            await _configService.UpdateLogLevelAsync(logLevel);
        }
        
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
    
    [ObservableProperty]
    private double _backgroundOpacity;
    
    public string BackgroundOpacityFormatted => BackgroundOpacity.ToString("P0");
    
    private readonly LoggerService _logger;

    public AppearanceSettingsViewModel(LoggerService logger,ConfigureService configService, ThemeService themeService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("AppearanceSettings");
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
            _configService.AppConfig.ThemeVariant == ThemeVariantMode.Light ? 
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
        
        _backgroundOpacity = _configService.AppConfig.BackgroundOpacity;
    }
    
    partial void OnBackgroundOpacityChanged(double value)
    {
        OnPropertyChanged(nameof(BackgroundOpacityFormatted));
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
        _configService.AppConfig.ThemeVariant = ThemeService.ToThemeVariantMode(themeVariant ?? "Default");
    
        _configService.AppConfig.TransparencyMode = SelectedTransparency switch
        {
            var t when t == TransparencyOptions[0] => TransparencyMode.Auto,
            var t when t == TransparencyOptions[1] => TransparencyMode.FullTransparency,
            var t when t == TransparencyOptions[2] => TransparencyMode.LightBackground,
            var t when t == TransparencyOptions[3] => TransparencyMode.DarkBackground,
            _ => TransparencyMode.Auto
        };
        
        _configService.AppConfig.BackgroundOpacity = BackgroundOpacity;
        
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

    private readonly LoggerService _logger;
    
    public LanguageSettingsViewModel(LoggerService logger,ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("LanguageSettings");
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
    private string _assistModel = "";
    
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
    
    private readonly LoggerService _logger;
    
    public AISettingsViewModel(LoggerService logger,ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("AISettings");
        _configService = configService;
        _localizationService = localizationService;
        
        // Load existing values if available
        ApiUrl = _configService.SystemConfig.OpenAiApiUrl ?? "https://api.deepseek.com/v1";
        ApiKey = _configService.SystemConfig.OpenAiApiKey ?? "";
        Model = _configService.SystemConfig.OpenAiModel ?? "deepseek-chat";
        AssistModel = _configService.SystemConfig.OpenAiAssistModel ?? "";
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
        _configService.SystemConfig.OpenAiAssistModel = AssistModel;
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
                OpenAiAssistModel = AssistModel,
                OpenAiModelTemperature = Temperature
            };
        
            // Create OpenAI client using the SDK
            var client = LibraryOpenKnowledge.Tools.AiTools.CreateOpenAiClient(tempConfig);
        
            _configService.AppStatistics.AddAiCallCount(_configService);
            // Test the main model
            var response = await LibraryOpenKnowledge.Tools.AiTools.SendChatMessageAsync(
                client,
                tempConfig,
                "Hello, this is a test message.",
                throwExceptions: true
            );
        
            // If main model is successful and assistant model is specified, test the assistant model
            if (!string.IsNullOrWhiteSpace(AssistModel))
            {
                // Temporary config with assistant model as main model for testing
                var assistConfig = new LibraryOpenKnowledge.Models.SystemConfig
                {
                    OpenAiApiUrl = ApiUrl,
                    OpenAiApiKey = ApiKey,
                    OpenAiModel = AssistModel,  // Use the assistant model here
                    OpenAiModelTemperature = Temperature
                };
                
                try
                {
                    _configService.AppStatistics.AddAiCallCount(_configService);
                    var assistResponse = await LibraryOpenKnowledge.Tools.AiTools.SendChatMessageAsync(
                        client,
                        assistConfig,
                        "Hello, this is a test message for the assistant model.",
                        throwExceptions: true
                    );
                    
                    // Both models tested successfully
                    ConnectionStatus = _localizationService["settings.ai.test.both.success"];
                    ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2ECC71"));
                }
                catch (Exception ex)
                {
                    // Main model succeeded but assistant model failed
                    ConnectionStatus = _localizationService["settings.ai.test.main.success.assist.fail"] + $": {ex.Message}";
                    ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F39C12"));
                }
            }
            else
            {
                // Only main model was tested (no assistant model specified)
                ConnectionStatus = _localizationService["settings.ai.test.success"];
                ConnectionStatusBackground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2ECC71"));
            }
        }
        catch (Exception ex)
        {
            // Main model test failed
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

public partial class PromptTemplatesViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;

    [ObservableProperty]
    private string _gradingTemplate;

    [ObservableProperty]
    private string _explanationTemplate;

    [ObservableProperty]
    private string _checkTemplate;

    [ObservableProperty]
    private bool _isGradingExpanded;

    [ObservableProperty]
    private bool _isExplanationExpanded;

    [ObservableProperty]
    private bool _isCheckExpanded;
    
    private readonly LoggerService _logger;

    public PromptTemplatesViewModel(LoggerService logger,ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("PromptTemplates");
        _configService = configService;
        _localizationService = localizationService;
        
        // Load existing values
        _gradingTemplate = _configService.AppConfig.PromptGradingTemplate;
        _explanationTemplate = _configService.AppConfig.PromptExplanationTemplate;
        _checkTemplate = _configService.AppConfig.PromptCheckTemplate;
    }

    public override async Task SaveAsync()
    {
        _configService.AppConfig.PromptGradingTemplate = GradingTemplate;
        _configService.AppConfig.PromptExplanationTemplate = ExplanationTemplate;
        _configService.AppConfig.PromptCheckTemplate = CheckTemplate;
        
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ToggleGradingExpanded() => IsGradingExpanded = !IsGradingExpanded;

    [RelayCommand]
    private void ToggleExplanationExpanded() => IsExplanationExpanded = !IsExplanationExpanded;

    [RelayCommand]
    private void ToggleCheckExpanded() => IsCheckExpanded = !IsCheckExpanded;
    
    [RelayCommand]
    private void ResetGradingTemplate()
    {
        GradingTemplate = PromptTemplateManager.DefaultGradingTemplate;
    }
    
    [RelayCommand]
    private void ResetExplanationTemplate()
    {
        ExplanationTemplate = PromptTemplateManager.DefaultExplanationTemplate;
    }
    
    [RelayCommand]
    private void ResetCheckTemplate()
    {
        CheckTemplate = PromptTemplateManager.DefaultErrorCheckTemplate;
    }
}

public partial class StorageSettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private string _cacheSize = "0 B";
    
    [ObservableProperty]
    private bool _isCacheCalculating = false;
    
    [ObservableProperty]
    private bool _isCacheClearing = false;
    
    [ObservableProperty]
    private string _logsSize = "0 B";
    
    [ObservableProperty]
    private bool _isLogsCalculating = false;
    
    [ObservableProperty]
    private bool _isLogsClearing = false;
    
    private readonly LoggerService _logger;
    
    public StorageSettingsViewModel(LoggerService logger,ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("StorageSettings");
        _configService = configService;
        _localizationService = localizationService;
        
        // Calculate initial cache size when the tab is created
        CalculateCacheSizeAsync().ConfigureAwait(false);
        CalculateLogsSizeAsync().ConfigureAwait(false);
    }
    
    [RelayCommand]
    private async Task CalculateCacheSizeAsync()
    {
        if (IsCacheCalculating)
            return;
            
        IsCacheCalculating = true;
        
        try
        {
            // Calculate cache size on a background thread
            var size = await Task.Run(ConfigureService.CalculateCacheSize);
            
            // Format the size in a human-readable format
            CacheSize = FormatFileSize(size);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error calculating cache size: {ex.Message}");
            _logger.Trace($"Error calculating cache size: {ex.StackTrace}");
            CacheSize = _localizationService["settings.storage.calculation.error"];
        }
        finally
        {
            IsCacheCalculating = false;
        }
    }
    
    [RelayCommand]
    private async Task CalculateLogsSizeAsync()
    {
        if (IsLogsCalculating)
            return;
            
        IsLogsCalculating = true;
        
        try
        {
            // Calculate cache size on a background thread
            var size = await Task.Run(ConfigureService.CalculateLogsSize);
            
            // Format the size in a human-readable format
            LogsSize = FormatFileSize(size);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error calculating logs size: {ex.Message}");
            _logger.Trace($"Error calculating logs size: {ex.StackTrace}");
            LogsSize = _localizationService["settings.storage.calculation.error"];
        }
        finally
        {
            IsLogsCalculating = false;
        }
    }
    
    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        if (IsCacheClearing)
            return;
            
        IsCacheClearing = true;
        
        try
        {
            // Clear cache on a background thread
            await Task.Run(() => ConfigureService.ClearCache());
            
            // Recalculate the cache size after clearing
            await CalculateCacheSizeAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error clearing cache: {ex.Message}");
            _logger.Trace($"Error clearing cache: {ex.StackTrace}");
        }
        finally
        {
            IsCacheClearing = false;
        }
    }
    
    [RelayCommand]
    private async Task ClearLogsAsync()
    {
        if (IsLogsClearing)
            return;
            
        IsLogsClearing = true;
        
        try
        {
            // Clear cache on a background thread
            await Task.Run(() => ConfigureService.ClearLogs(_logger.LogFilePath, true, throwExceptions: false));
            
            // Recalculate the cache size after clearing
            await CalculateLogsSizeAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error clearing logs: {ex.Message}");
            _logger.Trace($"Error clearing logs: {ex.StackTrace}");
        }
        finally
        {
            IsLogsClearing = false;
        }
    }
    
    public override async Task SaveAsync()
    {
        // Nothing to save for this view
        await Task.CompletedTask;
    }
    
    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;
        
        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }
        
        return $"{size:0.##} {suffixes[suffixIndex]}";
    }
}

public partial class AboutSettingsViewModel : SettingsViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private string _versionInfo;
    
    [ObservableProperty]
    private string _protocolVersion;
    
    private readonly LoggerService _logger;
    
    public AboutSettingsViewModel(LoggerService logger, ConfigureService configService, LocalizationService localizationService)
    {
        _logger = logger.CreateSubModule("AboutSettings");
        _configService = configService;
        _localizationService = localizationService;
        
        // Get version information from assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        if (version != null)
        {
            _versionInfo = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision:0000}";
        }
        else
        {
            _versionInfo = "1.0.0.0000";
        }
        
        // Get protocol version from DefaultClass
        var pVersion = LibraryOpenKnowledge.DefaultClass.CurrentVersion;
        _protocolVersion = $"Protocol {pVersion.Major}.{pVersion.Minor}.{pVersion.Patch}";
    }
    
    public override async Task SaveAsync()
    {
        // Nothing to save for this view
        await Task.CompletedTask;
    }
}
