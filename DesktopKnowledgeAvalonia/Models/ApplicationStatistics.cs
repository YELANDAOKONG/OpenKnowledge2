using System;
using System.Collections.Generic;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Utils;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationStatistics
{
    public long InitializationTime { get; set; } = TimeUtil.GetUnixTimestampMilliseconds();
    
    // 应用启动计数
    #region ApplicationStartCount
    
    public int ApplicationStartCount { get; set; } = 0;
    public Dictionary<int, int> ApplicationStartCountYears { get; set; } = new();
    
    public void AddApplicationStartCount()
    {
        ApplicationStartCount++;
        
        var year = TimeUtil.GetYear();
        if (!ApplicationStartCountYears.TryAdd(year, 1))
        {
            ApplicationStartCountYears[year]++;
        }
    }

    public void AddApplicationStartCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddApplicationStartCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }
    
    #endregion
    
    // 加载考试计数

    #region LoadExaminationCount
    
    public int LoadExaminationCount { get; set; } = 0;
    public Dictionary<int, int> LoadExaminationCountYears { get; set; } = new();
    
    public void AddLoadExaminationCount()
    {
        LoadExaminationCount++;
        
        var year = TimeUtil.GetYear();
        if (!LoadExaminationCountYears.TryAdd(year, 1))
        {
            LoadExaminationCountYears[year]++;
        }
    }
    
    public void AddLoadExaminationCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddLoadExaminationCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }
    
    #endregion
    
    // 提交考试计数

    #region SubmitExaminationCount
    
    public int SubmitExaminationCount { get; set; } = 0;
    public Dictionary<int, int> SubmitExaminationCountYears { get; set; } = new();
    
    public void AddSubmitExaminationCount()
    {
        SubmitExaminationCount++;
        
        var year = TimeUtil.GetYear();
        if (!SubmitExaminationCountYears.TryAdd(year, 1))
        {
            SubmitExaminationCountYears[year]++;
        }
    }
    
    public void AddSubmitExaminationCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddSubmitExaminationCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }
    
    #endregion

    #region AiCallCount
    
    // AI调用计数
    public int AiCallCount { get; set; } = 0;
    public Dictionary<int, int> AiCallCountYears { get; set; } = new();
    
    public void AddAiCallCount()
    {
        AiCallCount++;
        
        var year = TimeUtil.GetYear();
        if (!AiCallCountYears.TryAdd(year, 1))
        {
            AiCallCountYears[year]++;
        }
    }
    
    public void AddAiCallCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddAiCallCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }
    
    #endregion
}