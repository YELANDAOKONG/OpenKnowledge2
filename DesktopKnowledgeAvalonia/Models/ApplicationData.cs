using System;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationData
{
    public bool IsInExamination { get; set; } = false;
    public long? ExaminationTimer  { get; set; } = null;
    public Examination? CurrentExamination { get; set; } = null;
}