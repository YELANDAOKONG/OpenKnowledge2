using System.Text.Json;

namespace DesktopKnowledgeAvalonia.Services;

using System;
using System.IO;
using System.Threading.Tasks;
using LibraryOpenKnowledge.Models;
using DesktopKnowledgeAvalonia.Models;

public class ConfigureService
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = null,
        // MaxDepth = 64, // Increase max depth for complex objects
        // ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve // Handle circular references
    };
    
    private readonly string _configFilePath;
    private readonly string _dataFilePath;
    
    public SystemConfig SystemConfig { get; private set; } = new();
    public ApplicationConfig AppConfig { get; private set; } = new();
    public ApplicationData AppData { get; private set; } = new();
    
    // Constructor with optional path
    public ConfigureService(string? configFilePath = null, string? dataFilePath = null)
    {
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
        _dataFilePath = dataFilePath ?? GetDefaultAppDataPath();
        
        // Load synchronously but safely
        try
        {
            LoadConfigSync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error loading configuration: {ex}");
            // Fallback to defaults
            SystemConfig = new SystemConfig();
            AppConfig = new ApplicationConfig();
            AppData = new ApplicationData();
        }
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
    
    private static string GetDefaultAppDataPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dataFolder = Path.Combine(appDataPath, "OpenKnowledge", "Desktop");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
            
        return Path.Combine(dataFolder, "Data.json");
    }
    
    // Configuration model that combines both configs
    private class CombinedConfig
    {
        public SystemConfig System { get; set; } = new();
        public ApplicationConfig App { get; set; } = new();
    }
    
    private void LoadConfigSync()
    {
        // Load config
        if (File.Exists(_configFilePath))
        {
            try
            {
                string configJson = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<CombinedConfig>(configJson, _jsonOptions);
                
                if (config != null)
                {
                    SystemConfig = config.System ?? new SystemConfig();
                    AppConfig = config.App ?? new ApplicationConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config file: {ex.Message}");
                // Keep defaults
            }
        }
        // Load app data
        if (File.Exists(_dataFilePath))
        {
            try
            {
                string dataJson = File.ReadAllText(_dataFilePath);
                var data = JsonSerializer.Deserialize<ApplicationData>(dataJson, _jsonOptions);
                
                if (data != null)
                {
                    AppData = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading app data file: {ex.Message}");
                // Keep default AppData
            }
        }
    }
    
    // Load configuration from file asynchronously (for later use)
    private async Task LoadConfigAsync()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                await using var stream = File.OpenRead(_configFilePath);
                var config = await JsonSerializer.DeserializeAsync<CombinedConfig>(stream, _jsonOptions);
                
                if (config != null)
                {
                    SystemConfig = config.System ?? new SystemConfig();
                    AppConfig = config.App ?? new ApplicationConfig();
                }
            }
            if (File.Exists(_dataFilePath))
            {
                await using var stream = File.OpenRead(_dataFilePath);
                var data = await JsonSerializer.DeserializeAsync<ApplicationData>(stream, _jsonOptions);
                
                if (data != null)
                {
                    AppData = data;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            // Keep defaults
        }
    }
    
    // Save configuration to file asynchronously
    private async Task SaveConfigAsync()
    {
        // Save config file
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
        
            // Save config
            await using (var stream = File.Create(_configFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, config, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
        
        // Save data file separately
        try
        {
            // Save app data
            await using (var appdata = File.Create(_dataFilePath))
            {
                await JsonSerializer.SerializeAsync(appdata, AppData, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving app data: {ex.Message}");
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
    
    public async Task UpdateAppDataAsync(ApplicationData data)
    {
        AppData = data;
        await SaveConfigAsync();
    }
    
    // Manual save for when multiple properties are changed
    public async Task SaveChangesAsync()
    {
        await SaveConfigAsync();
    }
}
