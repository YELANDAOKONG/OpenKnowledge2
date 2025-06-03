using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace DesktopKnowledgeAvalonia.Services;

public class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _currentLanguage = "en-US";

    public event EventHandler? LanguageChanged;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value)
                return;
                
            _currentLanguage = value;
            ApplyLanguage();
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IEnumerable<string> AvailableLanguages => _translations.Keys;

    public LocalizationService()
    {
        LoadTranslations();
    }

    public string Translate(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var languageDict) &&
            languageDict.TryGetValue(key, out var translation))
            return translation;

        // Fallback to English
        if (_currentLanguage != "en-US" && 
            _translations.TryGetValue("en-US", out var enDict) &&
            enDict.TryGetValue(key, out var enTranslation))
            return enTranslation;

        // If all else fails, return the key itself
        return key;
    }

    private void LoadTranslations(bool throwExceptions = false)
    {
        try
        {
            var localesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Locales");
            if (!Directory.Exists(localesDirectory))
                Directory.CreateDirectory(localesDirectory);

            foreach (var file in Directory.GetFiles(localesDirectory, "*.json"))
            {
                var languageCode = Path.GetFileNameWithoutExtension(file);
                var jsonContent = File.ReadAllText(file);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                
                if (translations != null) _translations[languageCode] = translations;
            }

            // If no translations found, create at least an empty English one
            if (!_translations.ContainsKey("en-US"))
                _translations["en-US"] = new Dictionary<string, string>();
        }
        catch (Exception ex) when (!throwExceptions)
        {
            // In a real app, you'd want to log this error
            // TODO: Add error handling / Better Logger
            Console.WriteLine($"Error loading translations: {ex.Message}");
        }
    }

    // public void SaveTranslation(string languageCode, Dictionary<string, string> translations)
    // {
    //     var localesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Locales");
    //     if (!Directory.Exists(localesDirectory))
    //         Directory.CreateDirectory(localesDirectory);
    //
    //     var filePath = Path.Combine(localesDirectory, $"{languageCode}.json");
    //     var jsonContent = JsonSerializer.Serialize(translations, new JsonSerializerOptions
    //     {
    //         WriteIndented = true
    //     });
    //     
    //     File.WriteAllText(filePath, jsonContent);
    //     
    //     // Update in-memory translations
    //     _translations[languageCode] = translations;
    // }

    private void ApplyLanguage()
    {
        var culture = new CultureInfo(_currentLanguage);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
    
    public Dictionary<string, string> GetTranslations(string languageCode)
    {
        if (_translations.TryGetValue(languageCode, out var translations))
            return new Dictionary<string, string>(translations);
    
        return new Dictionary<string, string>();
    }
    
    public bool HasTranslation(string languageCode, string key)
    {
        return _translations.TryGetValue(languageCode, out var translations) && translations.ContainsKey(key);
    }
    
    public string this[string key] => Translate(key);

}
