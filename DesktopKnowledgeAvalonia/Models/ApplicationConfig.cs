using System;
using DesktopKnowledgeAvalonia.Tools;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationConfig
{
    public TransparencyMode TransparencyMode { get; set; } = TransparencyMode.FullTransparency;
    public string? ThemeVariant { get; set; } = "Dark";
    public string? PreferredLanguage { get; set; } = null;
    public string UserName { get; set; } = "Default";

    public string PromptGradingTemplate { get; set; }  = PromptTemplateManager.DefaultGradingTemplate;
    public string PromptExplanationTemplate { get; set; }  = PromptTemplateManager.DefaultExplanationTemplate;
    public string PromptCheckTemplate { get; set; }  = PromptTemplateManager.DefaultErrorCheckTemplate;
}