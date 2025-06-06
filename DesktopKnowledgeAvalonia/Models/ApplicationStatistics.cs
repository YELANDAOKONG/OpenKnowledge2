using System;
using DesktopKnowledgeAvalonia.Utils;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationStatistics
{
    public long InitializationTime { get; set; } = TimeUtil.GetUnixTimestampMilliseconds();
    
    public int ApplicationStartCount { get; set; } = 0;
    
    public int LoadExaminationCount { get; set; } = 0;
    public int SubmitExaminationCount { get; set; } = 0;
}