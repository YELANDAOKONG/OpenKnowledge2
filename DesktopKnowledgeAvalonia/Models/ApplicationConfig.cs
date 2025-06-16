using System;
using DesktopKnowledgeAvalonia.Tools;
using LibraryOpenKnowledge.Models;
using Microsoft.Extensions.Logging;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationConfig
{
    public TransparencyMode TransparencyMode { get; set; } = TransparencyMode.Auto;
    public string? ThemeVariant { get; set; } = "Dark";
    public string? PreferredLanguage { get; set; } = null;
    public string UserName { get; set; } = "Default";
    public string? AvatarFilePath { get; set; } = null;
    
    public bool EnableStatistics { get; set; } = true;
    public bool RandomizeWelcomeMessage { get; set; } = false;
    
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    public string PromptGradingTemplate { get; set; }  = PromptTemplateManager.DefaultGradingTemplate;
    public string PromptExplanationTemplate { get; set; }  = PromptTemplateManager.DefaultExplanationTemplate;
    public string PromptCheckTemplate { get; set; }  = PromptTemplateManager.DefaultErrorCheckTemplate;
}