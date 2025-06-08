using System;
using System.Collections.Generic;
using System.Globalization;
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
    public Dictionary<int, Dictionary<int, int>> ApplicationStartCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> ApplicationStartCountWeeks { get; set; } = new();
    
    public void AddApplicationStartCount()
    {
        ApplicationStartCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!ApplicationStartCountYears.TryAdd(year, 1))
        {
            ApplicationStartCountYears[year]++;
        }
        
        // 更新月度统计
        if (!ApplicationStartCountMonths.ContainsKey(year))
        {
            ApplicationStartCountMonths[year] = new Dictionary<int, int>();
        }
        if (!ApplicationStartCountMonths[year].TryAdd(month, 1))
        {
            ApplicationStartCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!ApplicationStartCountWeeks.ContainsKey(year))
        {
            ApplicationStartCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!ApplicationStartCountWeeks[year].TryAdd(week, 1))
        {
            ApplicationStartCountWeeks[year][week]++;
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
    public Dictionary<int, Dictionary<int, int>> LoadExaminationCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> LoadExaminationCountWeeks { get; set; } = new();
    
    public void AddLoadExaminationCount()
    {
        LoadExaminationCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!LoadExaminationCountYears.TryAdd(year, 1))
        {
            LoadExaminationCountYears[year]++;
        }
        
        // 更新月度统计
        if (!LoadExaminationCountMonths.ContainsKey(year))
        {
            LoadExaminationCountMonths[year] = new Dictionary<int, int>();
        }
        if (!LoadExaminationCountMonths[year].TryAdd(month, 1))
        {
            LoadExaminationCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!LoadExaminationCountWeeks.ContainsKey(year))
        {
            LoadExaminationCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!LoadExaminationCountWeeks[year].TryAdd(week, 1))
        {
            LoadExaminationCountWeeks[year][week]++;
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
    public Dictionary<int, Dictionary<int, int>> SubmitExaminationCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> SubmitExaminationCountWeeks { get; set; } = new();
    
    public void AddSubmitExaminationCount()
    {
        SubmitExaminationCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!SubmitExaminationCountYears.TryAdd(year, 1))
        {
            SubmitExaminationCountYears[year]++;
        }
        
        // 更新月度统计
        if (!SubmitExaminationCountMonths.ContainsKey(year))
        {
            SubmitExaminationCountMonths[year] = new Dictionary<int, int>();
        }
        if (!SubmitExaminationCountMonths[year].TryAdd(month, 1))
        {
            SubmitExaminationCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!SubmitExaminationCountWeeks.ContainsKey(year))
        {
            SubmitExaminationCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!SubmitExaminationCountWeeks[year].TryAdd(week, 1))
        {
            SubmitExaminationCountWeeks[year][week]++;
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

    // AI调用计数
    #region AiCallCount
    
    public int AiCallCount { get; set; } = 0;
    public Dictionary<int, int> AiCallCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> AiCallCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> AiCallCountWeeks { get; set; } = new();
    
    public void AddAiCallCount()
    {
        AiCallCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!AiCallCountYears.TryAdd(year, 1))
        {
            AiCallCountYears[year]++;
        }
        
        // 更新月度统计
        if (!AiCallCountMonths.ContainsKey(year))
        {
            AiCallCountMonths[year] = new Dictionary<int, int>();
        }
        if (!AiCallCountMonths[year].TryAdd(month, 1))
        {
            AiCallCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!AiCallCountWeeks.ContainsKey(year))
        {
            AiCallCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!AiCallCountWeeks[year].TryAdd(week, 1))
        {
            AiCallCountWeeks[year][week]++;
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
