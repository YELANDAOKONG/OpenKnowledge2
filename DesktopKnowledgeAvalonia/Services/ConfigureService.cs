namespace DesktopKnowledgeAvalonia.Services;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LibraryOpenKnowledge.Models;
using DesktopKnowledgeAvalonia.Models;

public class ConfigureService
{
    private readonly string _configFilePath;
    
    public SystemConfig SystemConfig { get; private set; } = new();
    public ApplicationConfig AppConfig { get; private set; } = new();
    
    // Constructor with optional path
    public ConfigureService(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
        
        // Synchronously wait for load to complete to ensure config is loaded
        LoadConfigAsync().GetAwaiter().GetResult();
    }
    
    // Get the default configuration file path
    private static string GetDefaultConfigPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configFolder = Path.Combine(appDataPath, "OpenKnowledge", "Desktop");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);
            
        return Path.Combine(configFolder, "Config.json");
    }
    
    // Configuration model that combines both configs
    private class CombinedConfig
    {
        public SystemConfig System { get; set; } = new();
        public ApplicationConfig App { get; set; } = new();
    }
    
    // Load configuration from file asynchronously
    private async Task LoadConfigAsync()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                using var stream = File.OpenRead(_configFilePath);
                var config = await JsonSerializer.DeserializeAsync<CombinedConfig>(stream);
                
                if (config != null)
                {
                    SystemConfig = config.System ?? new SystemConfig();
                    AppConfig = config.App ?? new ApplicationConfig();
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            
            // Use default configs
            SystemConfig = new SystemConfig();
            AppConfig = new ApplicationConfig();
        }
    }
    
    // Save configuration to file asynchronously
    private async Task SaveConfigAsync()
    {
        try
        {
            var config = new CombinedConfig
            {
                System = SystemConfig,
                App = AppConfig
            };
            
            var configDir = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
                
            using var stream = File.Create(_configFilePath);
            var options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, config, options);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            // TODO: Add error handling / Better Logger
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
    
    // AI configuration update methods with automatic saving
    public async Task UpdateAiApiUrlAsync(string? url)
    {
        SystemConfig.OpenAiApiUrl = url;
        await SaveConfigAsync();
    }
    
    public async Task UpdateAiApiKeyAsync(string? key)
    {
        SystemConfig.OpenAiApiKey = key;
        await SaveConfigAsync();
    }
    
    public async Task UpdateAiModelAsync(string? model)
    {
        SystemConfig.OpenAiModel = model;
        await SaveConfigAsync();
    }
    
    public async Task UpdateAiTemperatureAsync(double? temperature)
    {
        SystemConfig.OpenAiModelTemperature = temperature;
        await SaveConfigAsync();
    }
    
    // Update entire configs
    public async Task UpdateSystemConfigAsync(SystemConfig config)
    {
        SystemConfig = config;
        await SaveConfigAsync();
    }
    
    public async Task UpdateAppConfigAsync(ApplicationConfig config)
    {
        AppConfig = config;
        await SaveConfigAsync();
    }
    
    // Methods to update AppConfig properties would go here as the class is expanded
    
    // Manual save for when multiple properties are changed
    public async Task SaveChangesAsync()
    {
        await SaveConfigAsync();
    }
}
