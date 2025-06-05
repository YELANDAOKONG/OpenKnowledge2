using System.Text.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using LibraryOpenKnowledge.Models;
using DesktopKnowledgeAvalonia.Models;

namespace DesktopKnowledgeAvalonia.Services;

public class ConfigureService
{
    private readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = null
    };
    
    private readonly string _configFilePath;
    private readonly string _dataFilePath;
    
    public SystemConfig SystemConfig { get; private set; } = new();
    public ApplicationConfig AppConfig { get; private set; } = new();
    public ApplicationData AppData { get; private set; } = new();
    
    public ConfigureService(string? configFilePath = null, string? dataFilePath = null)
    {
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
        _dataFilePath = dataFilePath ?? GetDefaultAppDataPath();
        
        try
        {
            LoadConfigSync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error loading configuration: {ex}");
            SystemConfig = new SystemConfig();
            AppConfig = new ApplicationConfig();
            AppData = new ApplicationData();
        }
    }
    
    private static string GetDefaultConfigPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configFolder = Path.Combine(appDataPath, "OpenKnowledge", "Desktop");
        
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);
            
        return Path.Combine(configFolder, "Config.json");
    }
    
    private static string GetDefaultAppDataPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dataFolder = Path.Combine(appDataPath, "OpenKnowledge", "Desktop");
        
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
            
        return Path.Combine(dataFolder, "Data.json");
    }
    
    private class CombinedConfig
    {
        public SystemConfig System { get; set; } = new();
        public ApplicationConfig App { get; set; } = new();
    }
    
    private void LoadConfigSync()
    {
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
            }
        }
        
        if (File.Exists(_dataFilePath))
        {
            try
            {
                string dataJson = File.ReadAllText(_dataFilePath);
                var data = JsonSerializer.Deserialize<ApplicationData>(dataJson, _jsonOptions);
                
                if (data != null)
                    AppData = data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading app data file: {ex.Message}");
            }
        }
    }
    
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
                    AppData = data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }
    
    // Fixed file saving logic to avoid locking issues
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
        
            // Use atomic file writing pattern to avoid locking issues
            var configJson = JsonSerializer.Serialize(config, _jsonOptions);
            var tempConfigPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempConfigPath, configJson);
            File.Copy(tempConfigPath, _configFilePath, true);
            File.Delete(tempConfigPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
        
        // Save data file separately with the same pattern
        try
        {
            var appDataJson = JsonSerializer.Serialize(AppData, _jsonOptions);
            var tempDataPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempDataPath, appDataJson);
            File.Copy(tempDataPath, _dataFilePath, true);
            File.Delete(tempDataPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving app data: {ex.Message}");
        }
    }
    
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
    
    public async Task SaveChangesAsync() => await SaveConfigAsync();
}
