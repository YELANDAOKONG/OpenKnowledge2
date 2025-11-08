using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Services;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Utilities;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class InitializationViewModel : ViewModelBase
{
    private readonly ConfigureService _configureService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private string _apiUrl = "https://api.deepseek.com/v1";
    
    [ObservableProperty]
    private string _apiKey = "";
    
    [ObservableProperty]
    private string _model = "deepseek-chat";
    
    [ObservableProperty]
    private string _assistModel = "";
    
    [ObservableProperty]
    private double _temperature = 0.7;
    
    [ObservableProperty]
    private string? _apiUrlError;
    
    [ObservableProperty]
    private string? _apiKeyError;
    
    [ObservableProperty]
    private string? _modelError;
    
    [ObservableProperty]
    private bool _isTestingConnection;
    
    [ObservableProperty]
    private bool _isConnectionTested;
    
    [ObservableProperty]
    private string _connectionStatus = "";
    
    [ObservableProperty]
    private IBrush _connectionStatusBackground = new SolidColorBrush(Color.Parse("#569AFF"));
    
    public bool IsConfigurationComplete { get; private set; }
    
    public bool CanSave => 
        !string.IsNullOrWhiteSpace(ApiUrl) && 
        !string.IsNullOrWhiteSpace(ApiKey) && 
        !string.IsNullOrWhiteSpace(Model) &&
        Temperature >= 0 && Temperature <= 2;
    
    public string TemperatureFormatted => Temperature.ToString("F2");
    
    public event EventHandler? SaveCompleted;
    
    public InitializationViewModel()
    {
        _configureService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        
        // Load existing values if available
        if (!string.IsNullOrEmpty(_configureService.SystemConfig.OpenAiApiUrl))
            ApiUrl = _configureService.SystemConfig.OpenAiApiUrl;
            
        if (!string.IsNullOrEmpty(_configureService.SystemConfig.OpenAiApiKey))
            ApiKey = _configureService.SystemConfig.OpenAiApiKey;
            
        if (!string.IsNullOrEmpty(_configureService.SystemConfig.OpenAiModel))
            Model = _configureService.SystemConfig.OpenAiModel;
        
        if (!string.IsNullOrEmpty(_configureService.SystemConfig.OpenAiAssistModel))
            AssistModel = _configureService.SystemConfig.OpenAiAssistModel;
            
        if (_configureService.SystemConfig.OpenAiModelTemperature.HasValue)
            Temperature = _configureService.SystemConfig.OpenAiModelTemperature.Value;
            
        PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(ApiUrl) || 
                e.PropertyName == nameof(ApiKey) || 
                e.PropertyName == nameof(Model) ||
                e.PropertyName == nameof(Temperature))
            {
                OnPropertyChanged(nameof(CanSave));
            }
            
            if (e.PropertyName == nameof(Temperature))
            {
                OnPropertyChanged(nameof(TemperatureFormatted));
            }
        };
    }
    
    // Add these properties to track password visibility
    [ObservableProperty]
    private bool _isApiKeyVisible = false;

    public char? ApiKeyPasswordChar => IsApiKeyVisible ? null : '•';

    public string ApiKeyVisibilityIcon => IsApiKeyVisible 
        ? "M12 6c-4.5 0-8 3.5-8 8s3.5 8 8 8 8-3.5 8-8-3.5-8-8-8zm0 14c-3.3 0-6-2.7-6-6s2.7-6 6-6 6 2.7 6 6-2.7 6-6 6zm0-10c-2.2 0-4 1.8-4 4s1.8 4 4 4 4-1.8 4-4-1.8-4-4-4zm0 6c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2z"
        : "M12 6c-4.5 0-8 3.5-8 8s3.5 8 8 8 8-3.5 8-8-3.5-8-8-8zm0 14c-3.3 0-6-2.7-6-6s2.7-6 6-6 6 2.7 6 6-2.7 6-6 6zm-1-7.5h2v2h-2v-2zm0-6h2v5h-2v-5z";

    // Add a method to toggle password visibility
    [RelayCommand]
    private void ToggleApiKeyVisibility()
    {
        IsApiKeyVisible = !IsApiKeyVisible;
        OnPropertyChanged(nameof(ApiKeyPasswordChar));
        OnPropertyChanged(nameof(ApiKeyVisibilityIcon));
    }

    
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validate fields
        ApiUrlError = string.IsNullOrWhiteSpace(ApiUrl) 
            ? _localizationService["init.error.api.url.required"] 
            : null;
            
        ApiKeyError = string.IsNullOrWhiteSpace(ApiKey) 
            ? _localizationService["init.error.api.key.required"] 
            : null;
            
        ModelError = string.IsNullOrWhiteSpace(Model) 
            ? _localizationService["init.error.model.required"] 
            : null;
            
        if (ApiUrlError != null || ApiKeyError != null || ModelError != null)
            return;
            
        // Save configuration
        _configureService.SystemConfig.OpenAiApiUrl = ApiUrl;
        _configureService.SystemConfig.OpenAiApiKey = ApiKey;
        _configureService.SystemConfig.OpenAiModel = Model;
        _configureService.SystemConfig.OpenAiAssistModel = AssistModel;
        _configureService.SystemConfig.OpenAiModelTemperature = Temperature;
        
        await _configureService.SaveChangesAsync();
        
        IsConfigurationComplete = true;
        SaveCompleted?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        // Validate fields first
        if (string.IsNullOrWhiteSpace(ApiUrl) || 
            string.IsNullOrWhiteSpace(ApiKey) || 
            string.IsNullOrWhiteSpace(Model))
        {
            ConnectionStatus = _localizationService["init.test.connection.fill.all.fields"];
            ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#E74C3C"));
            IsConnectionTested = true;
            return;
        }

        IsTestingConnection = true;

        try
        {
            // Create a temporary SystemConfig with current values
            var tempConfig = new SystemConfig
            {
                OpenAiApiUrl = ApiUrl,
                OpenAiApiKey = ApiKey,
                OpenAiModel = Model,
                OpenAiAssistModel = AssistModel,
                OpenAiModelTemperature = Temperature
            };
        
            // Create OpenAI client using the SDK
            var client = AiHelper.CreateOpenAiClient(tempConfig);
        
            _configureService.AppStatistics.AddAiCallCount(_configureService);
            // Test the main model
            var response = await AiHelper.SendChatMessageAsync(
                client,
                tempConfig,
                "Hello, this is a test message.",
                throwExceptions: true
            );
        
            // If main model is successful and assistant model is specified, test the assistant model
            if (!string.IsNullOrWhiteSpace(AssistModel))
            {
                // Temporary config with assistant model as main model for testing
                var assistConfig = new SystemConfig
                {
                    OpenAiApiUrl = ApiUrl,
                    OpenAiApiKey = ApiKey,
                    OpenAiModel = AssistModel,  // Use the assistant model here
                    OpenAiModelTemperature = Temperature
                };
                
                try
                {
                    _configureService.AppStatistics.AddAiCallCount(_configureService);
                    var assistResponse = await AiHelper.SendChatMessageAsync(
                        client,
                        assistConfig,
                        "Hello, this is a test message for the assistant model.",
                        throwExceptions: true
                    );
                    
                    // Both models tested successfully
                    ConnectionStatus = _localizationService["init.test.connection.both.success"];
                    ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#2ECC71"));
                }
                catch (Exception ex)
                {
                    // Main model succeeded but assistant model failed
                    ConnectionStatus = _localizationService["init.test.connection.main.success.assist.fail"] + $": {ex.Message}";
                    ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#F39C12"));
                }
            }
            else
            {
                // Only main model was tested (no assistant model specified)
                ConnectionStatus = _localizationService["init.test.connection.success"];
                ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#2ECC71"));
            }
        }
        catch (Exception ex)
        {
            // Main model test failed
            ConnectionStatus = $"{_localizationService["init.test.connection.error"]}: {ex.Message}";
            ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#E74C3C"));
        }
        finally
        {
            IsTestingConnection = false;
            IsConnectionTested = true;
        }
    }

}
