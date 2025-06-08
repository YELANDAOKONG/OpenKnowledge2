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
    
    public int GetApplicationStartCount()
    {
        return ApplicationStartCount;
    }
    
    public int GetApplicationStartCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return ApplicationStartCountYears.GetValueOrDefault(dataYear, 0);
    }
    
    public int GetApplicationStartCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return ApplicationStartCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }
    
    public int GetApplicationStartCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);;
        return ApplicationStartCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
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
    
    public int GetAiCallCount()
    {
        return AiCallCount;
    }
    
    public int GetAiCallCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return AiCallCountYears.GetValueOrDefault(dataYear, 0);
    }
    
    public int GetAiCallCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return AiCallCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }
    
    public int GetAiCallCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);;
        return AiCallCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    #endregion
    
    // 题目交互计数
    #region QuestionInteractionCount

    public int QuestionInteractionCount { get; set; } = 0;
    public Dictionary<int, int> QuestionInteractionCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> QuestionInteractionCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> QuestionInteractionCountWeeks { get; set; } = new();

    public void AddQuestionInteractionCount()
    {
        QuestionInteractionCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!QuestionInteractionCountYears.TryAdd(year, 1))
        {
            QuestionInteractionCountYears[year]++;
        }
        
        // 更新月度统计
        if (!QuestionInteractionCountMonths.ContainsKey(year))
        {
            QuestionInteractionCountMonths[year] = new Dictionary<int, int>();
        }
        if (!QuestionInteractionCountMonths[year].TryAdd(month, 1))
        {
            QuestionInteractionCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!QuestionInteractionCountWeeks.ContainsKey(year))
        {
            QuestionInteractionCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!QuestionInteractionCountWeeks[year].TryAdd(week, 1))
        {
            QuestionInteractionCountWeeks[year][week]++;
        }
    }

    public void AddQuestionInteractionCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddQuestionInteractionCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    public int GetQuestionInteractionCount()
    {
        return QuestionInteractionCount;
    }

    public int GetQuestionInteractionCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return QuestionInteractionCountYears.GetValueOrDefault(dataYear, 0);
    }

    public int GetQuestionInteractionCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return QuestionInteractionCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }

    public int GetQuestionInteractionCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return QuestionInteractionCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
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
    
    public int GetLoadExaminationCount()
    {
        return LoadExaminationCount;
    }
    
    public int GetLoadExaminationCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return LoadExaminationCountYears.GetValueOrDefault(dataYear, 0);
    }
    
    public int GetLoadExaminationCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return LoadExaminationCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }
    
    public int GetLoadExaminationCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);;
        return LoadExaminationCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
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
    
    public int GetSubmitExaminationCount()
    {
        return SubmitExaminationCount;
    }
    
    public int GetSubmitExaminationCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return SubmitExaminationCountYears.TryGetValue(dataYear, out var count) ? count : 0;
    }
    
    public int GetSubmitExaminationCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return SubmitExaminationCountMonths.TryGetValue(dataYear, out var months) && months.TryGetValue(dataMonth, out var count) ? count : 0;
    }
    
    public int GetSubmitExaminationCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);;
        return SubmitExaminationCountWeeks.TryGetValue(dataYear, out var weeks) && weeks.TryGetValue(dataWeek, out var count) ? count : 0;
    }
    
    #endregion
    
    
    
}
