using System;
using DesktopKnowledgeAvalonia.Tools;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationConfig
{
    public TransparencyMode TransparencyMode { get; set; } = TransparencyMode.Auto;
    public ThemeVariantMode ThemeVariant { get; set; } = ThemeVariantMode.Dark;
    public double BackgroundOpacity { get; set; } = 0.38; 
    public string? PreferredLanguage { get; set; } = null;
    public string UserName { get; set; } = "Default";
    public string? AvatarFilePath { get; set; } = null;
    
    public bool EnableStatistics { get; set; } = true;
    public bool RandomizeWelcomeMessage { get; set; } = false;

    public string PromptGradingTemplate { get; set; }  = PromptTemplateManager.DefaultGradingTemplate;
    public string PromptExplanationTemplate { get; set; }  = PromptTemplateManager.DefaultExplanationTemplate;
    public string PromptCheckTemplate { get; set; }  = PromptTemplateManager.DefaultErrorCheckTemplate;
}