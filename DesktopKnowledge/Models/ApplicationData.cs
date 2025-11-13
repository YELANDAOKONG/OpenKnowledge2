using System;
using OpenKnowledge.Models;

namespace DesktopKnowledge.Models;

[Serializable]
public class ApplicationData
{
    // 考试相关
    public bool IsInExamination { get; set; } = false;
    public bool IsTheExaminationStarted { get; set; } = false;
    public long? ExaminationTimer  { get; set; } = null;
    public long AccumulatedExaminationTime { get; set; } = 0;
    public Examination? CurrentExamination { get; set; } = null;
    
    // 学习模式相关
    public bool IsInStudy { get; set; } = false;
    public bool IsTheStudyStarted { get; set; } = false;
    public long? StudyTimer  { get; set; } = null;
    public long AccumulatedStudyTime { get; set; } = 0;
    public Examination? CurrentStudy { get; set; } = null;
    
}