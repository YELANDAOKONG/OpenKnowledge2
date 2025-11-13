using System.Text.Json;
using OpenKnowledge.Models;

namespace OpenKnowledge.Utilities;

public class ConfigHelper
{
    /// <summary>
    /// 获取用户主目录的路径
    /// </summary>
    /// <returns>用户主目录的完整路径</returns>
    private static string GetUserHomeDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    
    /// <summary>
    /// 从指定文件名获取完整配置文件路径
    /// </summary>
    /// <param name="fileName">配置文件名</param>
    /// <returns>配置文件的完整路径</returns>
    public static string GetConfigFilePath(string fileName)
    {
        return Path.Combine(GetUserHomeDirectory(), ".config", fileName);
    }

    #region JsonSerialize
    
    /// <summary>
    /// 使用 System.Text.Json 将对象序列化为 JSON 字符串
    /// </summary>
    /// <typeparam name="T">要序列化的对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>JSON 字符串</returns>
    public static string SerializeWithSystemTextJson<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return System.Text.Json.JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// 使用 Newtonsoft.Json 将对象序列化为 JSON 字符串
    /// </summary>
    /// <typeparam name="T">要序列化的对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>JSON 字符串</returns>
    public static string SerializeWithNewtonsoftJson<T>(T obj)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
    }

    /// <summary>
    /// 使用 System.Text.Json 将 JSON 字符串反序列化为对象
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <returns>反序列化后的对象</returns>
    public static T? DeserializeWithSystemTextJson<T>(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// 使用 Newtonsoft.Json 将 JSON 字符串反序列化为对象
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    /// <returns>反序列化后的对象</returns>
    public static T? DeserializeWithNewtonsoftJson<T>(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }

    #endregion
    
    /// <summary>
    /// 将配置保存到指定文件
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="config">配置对象</param>
    /// <param name="fileName">文件名</param>
    /// <param name="useNewtonsoftJson">是否使用 Newtonsoft.Json</param>
    public static void SaveConfig<T>(T config, string fileName, bool useNewtonsoftJson = false)
    {
        string filePath = GetConfigFilePath(fileName);
        string? path = Path.GetDirectoryName(filePath);
        if (path != null)
        {
            Directory.CreateDirectory(path);
        }
        
        string json = useNewtonsoftJson 
            ? SerializeWithNewtonsoftJson(config) 
            : SerializeWithSystemTextJson(config);
        
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从指定文件加载配置
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="fileName">文件名</param>
    /// <param name="useNewtonsoftJson">是否使用 Newtonsoft.Json</param>
    /// <returns>配置对象</returns>
    public static T? LoadConfig<T>(string fileName, bool useNewtonsoftJson = false)
    {
        string filePath = GetConfigFilePath(fileName);
        
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        string json = File.ReadAllText(filePath);
        
        return useNewtonsoftJson 
            ? DeserializeWithNewtonsoftJson<T>(json) 
            : DeserializeWithSystemTextJson<T>(json);
    }

    /// <summary>
    /// 获取或创建系统配置
    /// </summary>
    /// <param name="fileName">配置文件名</param>
    /// <param name="useNewtonsoftJson">是否使用 Newtonsoft.Json</param>
    /// <returns>系统配置对象</returns>
    public static SystemConfig GetOrCreateSystemConfig(string fileName = "open-knowledge.json", bool useNewtonsoftJson = false)
    {
        var config = LoadConfig<SystemConfig>(fileName, useNewtonsoftJson);
        
        if (config == null)
        {
            config = new SystemConfig();
            SaveConfig(config, fileName, useNewtonsoftJson);
        }
        
        return config;
    }
}

