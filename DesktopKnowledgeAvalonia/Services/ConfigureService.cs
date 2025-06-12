using System.Text.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibraryOpenKnowledge.Models;
using DesktopKnowledgeAvalonia.Models;

namespace DesktopKnowledgeAvalonia.Services;

public class ConfigureService
{
    public static readonly string ApplicationName = "OpenKnowledge";
    
    private readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = null
    };
    
    private readonly string _configFilePath;
    private readonly string _dataFilePath;
    private readonly string _statisticsFilePath;
    
    public SystemConfig SystemConfig { get; private set; } = new();
    public ApplicationConfig AppConfig { get; private set; } = new();
    public ApplicationData AppData { get; private set; } = new();
    public ApplicationStatistics AppStatistics { get; set; } = new();
    
    public ConfigureService(string? configFilePath = null, string? dataFilePath = null, string? statisticsFilePath = null)
    {
        _configFilePath = configFilePath ?? GetDefaultConfigPath();
        _dataFilePath = dataFilePath ?? GetDefaultAppDataPath();
        _statisticsFilePath = statisticsFilePath ?? GetDefaultStatisticsPath();
        
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
            AppStatistics = new ApplicationStatistics();
        }
    }
    
    public static string GetRootDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appDataPath, ApplicationName);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }
    
    public static string GetRootTempDirectory()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), ApplicationName);
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }
    
    public static string GetConfigDirectory()
    {
        var root = GetRootDirectory();
        var folder = Path.Combine(root, "Desktop");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }
    
    public static string GetAvatarDirectory()
    {
        var rootPath = GetConfigDirectory();
        var folder = Path.Combine(rootPath, "Avatars");
        if (!File.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }
    
    public static Task ClearAvatars(string? currentAvatarPath)
    { 
        var path = GetAvatarDirectory();
        foreach (var file in Directory.GetFiles(path))
        {
            if (currentAvatarPath == null || !file.Equals(currentAvatarPath))
            {
                File.Delete(file);
            }
        }

        return Task.CompletedTask;
    }

    public static string GetCacheDirectory()
    {
        var root = GetRootDirectory();
        var folder = Path.Combine(root, "Cache");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }
    
    public static string GetTempDirectory()
    {
        var root = GetRootTempDirectory();
        var tempFolder = Path.Combine(root, "Temp");
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }
    
    public static string GetLogsDirectory()
    {
        var root = GetRootDirectory();
        var folder = Path.Combine(root, "Logs");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }
    
    public static string GetDefaultConfigPath()
    {
        var configFolder = GetConfigDirectory();
        
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);
            
        return Path.Combine(configFolder, "Config.json");
    }
    
    public static string GetDefaultAppDataPath()
    {
        var dataFolder = GetConfigDirectory();
        
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
            
        return Path.Combine(dataFolder, "Data.json");
    }
    
    public static string GetDefaultStatisticsPath()
    {
        var root = GetRootDirectory();
        var statisticsFolder = Path.Combine(root, "Desktop");
        
        if (!Directory.Exists(statisticsFolder))
            Directory.CreateDirectory(statisticsFolder);
            
        return Path.Combine(statisticsFolder, "Statistics.json");
    }
    
    public static void ClearCache(bool throwExceptions = false)
    {
        try
        {
            var cacheFolder = GetCacheDirectory();
            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }
        }
        catch (Exception) when(!throwExceptions)
        {
            return;
        }
    }
    
    public static long CalculateCacheSize()
    {
        var cacheFolder = GetCacheDirectory();
        if (!Directory.Exists(cacheFolder)) return 0;
        
        long size = 0;
        foreach (var file in Directory.GetFiles(cacheFolder, "*", SearchOption.AllDirectories))
        {
            size += new FileInfo(file).Length;
        }
        return size;
    }

    /// <summary>
    /// 清理日志文件夹
    /// </summary>
    /// <param name="currentLogFilePath">当前正在使用的日志文件路径</param>
    /// <param name="ignoreIfCanNotDelete">无法删除文件时是否忽略</param>
    /// <param name="throwExceptions">是否向上抛出异常</param>
    public static void ClearLogs(string? currentLogFilePath = null, bool ignoreIfCanNotDelete = true, bool throwExceptions = false)
    {
        try
        {
            var logsFolder = GetLogsDirectory();
            if (!Directory.Exists(logsFolder))
                return;

            // 规范化当前日志文件路径用于比较
            string? normalizedCurrentLogPath = null;
            if (!string.IsNullOrEmpty(currentLogFilePath))
            {
                normalizedCurrentLogPath = Path.GetFullPath(currentLogFilePath);
            }
            
            var files = Directory.GetFiles(logsFolder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                // 跳过当前正在使用的日志文件
                if (normalizedCurrentLogPath != null && 
                    Path.GetFullPath(file).Equals(normalizedCurrentLogPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    if (!ignoreIfCanNotDelete)
                    {
                        if (throwExceptions)
                            throw;
                        return;
                    }
                    continue;
                }
            }
            var directories = Directory.GetDirectories(
                    logsFolder, 
                    "*",
                    SearchOption.AllDirectories
                    ).OrderByDescending(d => d.Length);

            foreach (var dir in directories)
            {
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                    }
                }
                catch (Exception)
                {
                    if (!ignoreIfCanNotDelete)
                    {
                        if (throwExceptions)
                            throw;
                        break; 
                    }
                    continue;
                }
            }
            
            try
            {
                if (!Directory.EnumerateFileSystemEntries(logsFolder).Any())
                {
                    Directory.Delete(logsFolder);
                }
            }
            catch (Exception)
            {
                if (!ignoreIfCanNotDelete && throwExceptions) throw;
            }
        }
        catch (Exception) when (!throwExceptions)
        {
            return;
        }
    }

    
    public static long CalculateLogsSize()
    {
        var logsFolder = GetLogsDirectory();
        if (!Directory.Exists(logsFolder)) return 0;
        
        long size = 0;
        foreach (var file in Directory.GetFiles(logsFolder, "*", SearchOption.AllDirectories))
        {
            size += new FileInfo(file).Length;
        }
        return size;
    }

    public static string RandomFileName(string prefix = "temp", string extension = "dat")
    {
        var random = Random.Shared;
        var rId1 = random.Next(Int32.MaxValue);
        var rId2 = random.Next(Int32.MaxValue);
        var rId3 = random.Next(Int32.MaxValue);
        var times = DateTime.UtcNow.Second;
        var timems = DateTime.UtcNow.Microsecond;
        var timens = DateTime.UtcNow.Nanosecond;
        if (extension.StartsWith("."))
        {
            extension = extension.Substring(1);
        }
        return $"{prefix}_{rId1}{rId2}{rId3}_{times}{timems}{timens}.{extension}";
    }
    
    public static string RandomDirectoryName(string prefix = "temp")
    {
        var random = Random.Shared;
        var rId1 = random.Next(Int32.MaxValue);
        var rId2 = random.Next(Int32.MaxValue);
        var rId3 = random.Next(Int32.MaxValue);

        var times = DateTime.UtcNow.Second;
        var timems = DateTime.UtcNow.Microsecond;
        var timens = DateTime.UtcNow.Nanosecond;
        return $"{prefix}_{rId1}{rId2}{rId3}_{times}{timems}{timens}";
    }
    
    public static string NewTempFilePath(string prefix = "temp", string extension = ".dat")
    {
        var tempFolder = GetTempDirectory();
        var fileName = RandomFileName(prefix, extension);
        var filePath = Path.Combine(tempFolder, fileName);
        return filePath;
    }
    
    public static string NewTempDirectoryPath(string prefix = "temp")
    {
        var tempFolder = GetTempDirectory();
        var dirName = RandomDirectoryName(prefix);
        var dirPath = Path.Combine(tempFolder, dirName);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        return dirPath;
    }
    
    public static string RandomLogFileName(string prefix = "log", string extension = "log")
    {
        var random = Random.Shared;
        var rId = random.Next(Int32.MaxValue);
        
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var time = DateTime.UtcNow.ToString("HHmmss");
        var times = DateTime.UtcNow.Second;
        var timems = DateTime.UtcNow.Microsecond;
        var timens = DateTime.UtcNow.Nanosecond;
        return $"{prefix}_{date}{time}_{times}{timems}{timens}_{rId}.{extension}";
    }
    
    public static string NewLogFilePath(string prefix = "log", string extension = "log")
    {
        var tempFolder = GetLogsDirectory();
        var fileName = RandomLogFileName(prefix, extension);
        var filePath = Path.Combine(tempFolder, fileName);
        return filePath;
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
        
        if (File.Exists(_statisticsFilePath))
        {
            try
            {
                string statisticsJson = File.ReadAllText(_statisticsFilePath);
                var statistics = JsonSerializer.Deserialize<ApplicationStatistics>(statisticsJson, _jsonOptions);
                
                if (statistics != null)
                    AppStatistics = statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading statistics file: {ex.Message}");
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
            
            if (File.Exists(_statisticsFilePath))
            {
                await using var stream = File.OpenRead(_statisticsFilePath);
                var statistics = await JsonSerializer.DeserializeAsync<ApplicationStatistics>(stream, _jsonOptions);
                
                if (statistics != null)
                    AppStatistics = statistics;
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
        
        // Save statistics file separately with the same pattern
        try
        {
            var statisticsJson = JsonSerializer.Serialize(AppStatistics, _jsonOptions);
            var tempStatisticsPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempStatisticsPath, statisticsJson);
            File.Copy(tempStatisticsPath, _statisticsFilePath, true);
            File.Delete(tempStatisticsPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving statistics: {ex.Message}");
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
    
    public async Task UpdateAppStatisticsAsync(ApplicationStatistics statistics)
    {
        AppStatistics = statistics;
        await SaveConfigAsync();
    }
    
    public async Task SaveChangesAsync() => await SaveConfigAsync();
}
