using System;
using System.Collections.Generic;
using DesktopKnowledgeAvalonia.Utils;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationStatistics
{
    public long InitializationTime { get; set; } = TimeUtil.GetUnixTimestampMilliseconds();
    
    public int ApplicationStartCount { get; set; } = 0;
    
    public int LoadExaminationCount { get; set; } = 0;
    public int SubmitExaminationCount { get; set; } = 0;
    
    public Dictionary<int, int> ApplicationStartCountYears { get; set; } = new();
    public Dictionary<int, int> LoadExaminationCountYears { get; set; } = new();
    public Dictionary<int, int> SubmitExaminationCountYears { get; set; } = new();
    
    public void AddApplicationStartCount()
    {
        ApplicationStartCount++;
        
        var year = TimeUtil.GetYear();
        if (!ApplicationStartCountYears.TryAdd(year, 1))
        {
            ApplicationStartCountYears[year]++;
        }
    }
    
    public void AddLoadExaminationCount()
    {
        LoadExaminationCount++;
        
        var year = TimeUtil.GetYear();
        if (!LoadExaminationCountYears.TryAdd(year, 1))
        {
            LoadExaminationCountYears[year]++;
        }
    }
    
    public void AddSubmitExaminationCount()
    {
        SubmitExaminationCount++;
        
        var year = TimeUtil.GetYear();
        if (!SubmitExaminationCountYears.TryAdd(year, 1))
        {
            SubmitExaminationCountYears[year]++;
        }
    }
}