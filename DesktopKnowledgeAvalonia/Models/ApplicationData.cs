using System;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationData
{
    public bool IsInExamination { get; set; } = false;
    public Examination? CurrentExamination { get; set; } = null;
}