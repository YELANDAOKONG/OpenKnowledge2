using System;
using LibraryOpenKnowledge.Models;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationData
{
    // 考试相关
    public bool IsInExamination { get; set; } = false;
    public bool IsTheExaminationStarted { get; set; } = false;
    public long? ExaminationTimer  { get; set; } = null;
    public Examination? CurrentExamination { get; set; } = null;
    
    // 学习模式相关
    public bool IsInStudy { get; set; } = false;
    public bool IsTheStudyStarted { get; set; } = false;
    public long? StudyTimer  { get; set; } = null;
    public Examination? CurrentStudy { get; set; } = null;
    
}