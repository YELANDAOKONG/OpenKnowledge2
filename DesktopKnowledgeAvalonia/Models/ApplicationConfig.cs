using System;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationConfig
{
    public string? PreferredLanguage { get; set; } = null;
    public string UserName { get; set; } = "Default";
}