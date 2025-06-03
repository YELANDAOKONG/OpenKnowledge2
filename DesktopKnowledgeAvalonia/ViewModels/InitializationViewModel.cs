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
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            
            // Build simple test request
            var requestObj = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = "Hello!" }
                },
                max_tokens = 10,
                temperature = Temperature
            };
            
            var json = JsonSerializer.Serialize(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var testUrl = ApiUrl;
            if (!testUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            {
                testUrl = testUrl.TrimEnd('/') + "/chat/completions";
            }
            
            var response = await client.PostAsync(testUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                ConnectionStatus = _localizationService["init.test.connection.success"];
                ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#2ECC71"));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ConnectionStatus = $"{_localizationService["init.test.connection.error"]} ({response.StatusCode}): {errorContent}";
                ConnectionStatusBackground = new SolidColorBrush(Color.Parse("#E74C3C"));
            }
        }
        catch (Exception ex)
        {
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
