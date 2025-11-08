using System;
using System.Collections.Generic;
using System.Globalization;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Utils;

namespace DesktopKnowledgeAvalonia.Models;

[Serializable]
public class ApplicationStatistics
{
    public long InitializationTime { get; set; } = TimeHelper.GetUnixTimestampMilliseconds();
    
    // 应用启动计数
    #region ApplicationStartCount
    
    public int ApplicationStartCount { get; set; } = 0;
    public Dictionary<int, int> ApplicationStartCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> ApplicationStartCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> ApplicationStartCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> ApplicationStartCountDays { get; set; } = new();
    
    public void AddApplicationStartCount()
    {
        ApplicationStartCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
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
        
        // 更新日统计
        if (!ApplicationStartCountDays.ContainsKey(year))
        {
            ApplicationStartCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!ApplicationStartCountDays[year].ContainsKey(month))
        {
            ApplicationStartCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!ApplicationStartCountDays[year][month].TryAdd(day, 1))
        {
            ApplicationStartCountDays[year][month][day]++;
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
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return ApplicationStartCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetApplicationStartCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return ApplicationStartCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }
    
    #endregion
    
    // AI调用计数
    #region AiCallCount
    
    public int AiCallCount { get; set; } = 0;
    public Dictionary<int, int> AiCallCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> AiCallCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> AiCallCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> AiCallCountDays { get; set; } = new();
    
    public void AddAiCallCount()
    {
        AiCallCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
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
        
        // 更新日统计
        if (!AiCallCountDays.ContainsKey(year))
        {
            AiCallCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!AiCallCountDays[year].ContainsKey(month))
        {
            AiCallCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!AiCallCountDays[year][month].TryAdd(day, 1))
        {
            AiCallCountDays[year][month][day]++;
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
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return AiCallCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetAiCallCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return AiCallCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }
    
    #endregion
    
    // 题目交互计数
    #region QuestionInteractionCount

    public int QuestionInteractionCount { get; set; } = 0;
    public Dictionary<int, int> QuestionInteractionCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> QuestionInteractionCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> QuestionInteractionCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> QuestionInteractionCountDays { get; set; } = new();

    public void AddQuestionInteractionCount()
    {
        QuestionInteractionCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
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
        
        // 更新日统计
        if (!QuestionInteractionCountDays.ContainsKey(year))
        {
            QuestionInteractionCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!QuestionInteractionCountDays[year].ContainsKey(month))
        {
            QuestionInteractionCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!QuestionInteractionCountDays[year][month].TryAdd(day, 1))
        {
            QuestionInteractionCountDays[year][month][day]++;
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
    
    public int GetQuestionInteractionCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return QuestionInteractionCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }

    #endregion
    
    
    // 加载考试计数
    #region LoadExaminationCount
    
    public int LoadExaminationCount { get; set; } = 0;
    public Dictionary<int, int> LoadExaminationCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> LoadExaminationCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> LoadExaminationCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> LoadExaminationCountDays { get; set; } = new();
    
    public void AddLoadExaminationCount()
    {
        LoadExaminationCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
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
        
        // 更新日统计
        if (!LoadExaminationCountDays.ContainsKey(year))
        {
            LoadExaminationCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!LoadExaminationCountDays[year].ContainsKey(month))
        {
            LoadExaminationCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!LoadExaminationCountDays[year][month].TryAdd(day, 1))
        {
            LoadExaminationCountDays[year][month][day]++;
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
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return LoadExaminationCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetLoadExaminationCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return LoadExaminationCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }
    
    #endregion
    
    // 提交考试计数
    #region SubmitExaminationCount
    
    public int SubmitExaminationCount { get; set; } = 0;
    public Dictionary<int, int> SubmitExaminationCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> SubmitExaminationCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> SubmitExaminationCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> SubmitExaminationCountDays { get; set; } = new();
    
    public void AddSubmitExaminationCount()
    {
        SubmitExaminationCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
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
        
        // 更新日统计
        if (!SubmitExaminationCountDays.ContainsKey(year))
        {
            SubmitExaminationCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!SubmitExaminationCountDays[year].ContainsKey(month))
        {
            SubmitExaminationCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!SubmitExaminationCountDays[year][month].TryAdd(day, 1))
        {
            SubmitExaminationCountDays[year][month][day]++;
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
        return SubmitExaminationCountYears.GetValueOrDefault(dataYear, 0);
    }
    
    public int GetSubmitExaminationCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return SubmitExaminationCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }
    
    public int GetSubmitExaminationCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return SubmitExaminationCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetSubmitExaminationCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return SubmitExaminationCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }
    
    #endregion
    
    // 启动学习计数
    #region StartStudyCount

    public int StartStudyCount { get; set; } = 0;
    public Dictionary<int, int> StartStudyCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> StartStudyCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> StartStudyCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> StartStudyCountDays { get; set; } = new();

    public void AddStartStudyCount()
    {
        StartStudyCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!StartStudyCountYears.TryAdd(year, 1))
        {
            StartStudyCountYears[year]++;
        }
        
        // 更新月度统计
        if (!StartStudyCountMonths.ContainsKey(year))
        {
            StartStudyCountMonths[year] = new Dictionary<int, int>();
        }
        if (!StartStudyCountMonths[year].TryAdd(month, 1))
        {
            StartStudyCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!StartStudyCountWeeks.ContainsKey(year))
        {
            StartStudyCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!StartStudyCountWeeks[year].TryAdd(week, 1))
        {
            StartStudyCountWeeks[year][week]++;
        }
        
        // 更新日统计
        if (!StartStudyCountDays.ContainsKey(year))
        {
            StartStudyCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!StartStudyCountDays[year].ContainsKey(month))
        {
            StartStudyCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!StartStudyCountDays[year][month].TryAdd(day, 1))
        {
            StartStudyCountDays[year][month][day]++;
        }
    }

    public void AddStartStudyCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddStartStudyCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    public int GetStartStudyCount()
    {
        return StartStudyCount;
    }

    public int GetStartStudyCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return StartStudyCountYears.GetValueOrDefault(dataYear, 0);
    }

    public int GetStartStudyCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return StartStudyCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }

    public int GetStartStudyCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return StartStudyCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetStartStudyCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return StartStudyCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }

    #endregion

    // 完成学习计数
    #region CompleteStudyCount

    public int CompleteStudyCount { get; set; } = 0;
    public Dictionary<int, int> CompleteStudyCountYears { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> CompleteStudyCountMonths { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> CompleteStudyCountWeeks { get; set; } = new();
    public Dictionary<int, Dictionary<int, Dictionary<int, int>>> CompleteStudyCountDays { get; set; } = new();

    public void AddCompleteStudyCount()
    {
        CompleteStudyCount++;
        
        DateTime utcNow = DateTime.UtcNow;
        int year = utcNow.Year;
        int month = utcNow.Month;
        int day = utcNow.Day;
        
        // 获取当前UTC时间所在的ISO周数
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int week = calendar.GetWeekOfYear(utcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        // 更新年度统计
        if (!CompleteStudyCountYears.TryAdd(year, 1))
        {
            CompleteStudyCountYears[year]++;
        }
        
        // 更新月度统计
        if (!CompleteStudyCountMonths.ContainsKey(year))
        {
            CompleteStudyCountMonths[year] = new Dictionary<int, int>();
        }
        if (!CompleteStudyCountMonths[year].TryAdd(month, 1))
        {
            CompleteStudyCountMonths[year][month]++;
        }
        
        // 更新周统计
        if (!CompleteStudyCountWeeks.ContainsKey(year))
        {
            CompleteStudyCountWeeks[year] = new Dictionary<int, int>();
        }
        if (!CompleteStudyCountWeeks[year].TryAdd(week, 1))
        {
            CompleteStudyCountWeeks[year][week]++;
        }
        
        // 更新日统计
        if (!CompleteStudyCountDays.ContainsKey(year))
        {
            CompleteStudyCountDays[year] = new Dictionary<int, Dictionary<int, int>>();
        }
        if (!CompleteStudyCountDays[year].ContainsKey(month))
        {
            CompleteStudyCountDays[year][month] = new Dictionary<int, int>();
        }
        if (!CompleteStudyCountDays[year][month].TryAdd(day, 1))
        {
            CompleteStudyCountDays[year][month][day]++;
        }
    }

    public void AddCompleteStudyCount(ConfigureService service, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddCompleteStudyCount();
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    public int GetCompleteStudyCount()
    {
        return CompleteStudyCount;
    }

    public int GetCompleteStudyCountYear(int? year = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        return CompleteStudyCountYears.GetValueOrDefault(dataYear, 0);
    }

    public int GetCompleteStudyCountMonth(int? year = null, int? month = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        return CompleteStudyCountMonths.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataMonth, 0);
    }

    public int GetCompleteStudyCountWeek(int? year = null, int? week = null)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataWeek = week ?? calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return CompleteStudyCountWeeks.GetValueOrDefault(dataYear, new Dictionary<int, int>()).GetValueOrDefault(dataWeek, 0);
    }
    
    public int GetCompleteStudyCountDay(int? year = null, int? month = null, int? day = null)
    {
        var dataYear = year ?? DateTime.UtcNow.Year;
        var dataMonth = month ?? DateTime.UtcNow.Month;
        var dataDay = day ?? DateTime.UtcNow.Day;
        return CompleteStudyCountDays
            .GetValueOrDefault(dataYear, new Dictionary<int, Dictionary<int, int>>())
            .GetValueOrDefault(dataMonth, new Dictionary<int, int>())
            .GetValueOrDefault(dataDay, 0);
    }

    #endregion

    
    
    // 考试时间计时（毫秒）
    #region ExaminationTime

    public long ExaminationTime { get; set; } = 0;

    // 累加考试时间（毫秒）
    public void AddExaminationTime(long milliseconds)
    {
        ExaminationTime += milliseconds;
    }

    // 带服务的考试时间累加
    public void AddExaminationTime(ConfigureService service, long milliseconds, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddExaminationTime(milliseconds);
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    // 获取考试时间（毫秒）
    public long GetExaminationTime()
    {
        return ExaminationTime;
    }

    #endregion

    // 学习时间计时（毫秒）
    #region StudyTime

    public long StudyTime { get; set; } = 0;

    // 累加学习时间（毫秒）
    public void AddStudyTime(long milliseconds)
    {
        StudyTime += milliseconds;
    }

    // 带服务的学习时间累加
    public void AddStudyTime(ConfigureService service, long milliseconds, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddStudyTime(milliseconds);
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    // 获取学习时间（毫秒）
    public long GetStudyTime()
    {
        return StudyTime;
    }

    #endregion

    // 应用运行时间（毫秒）
    #region ApplicationRunTime

    public long ApplicationRunTime { get; set; } = 0;

    // 累加应用运行时间（毫秒）
    public void AddApplicationRunTime(long milliseconds)
    {
        ApplicationRunTime += milliseconds;
    }

    // 设置应用运行时间（毫秒）
    public void SetApplicationRunTime(long milliseconds)
    {
        ApplicationRunTime = milliseconds;
    }

    // 带服务的应用运行时间累加
    public void AddApplicationRunTime(ConfigureService service, long milliseconds, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            AddApplicationRunTime(milliseconds);
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    // 带服务的应用运行时间设置
    public void SetApplicationRunTime(ConfigureService service, long milliseconds, bool saveChanges = true)
    {
        if (service.AppConfig.EnableStatistics)
        {
            SetApplicationRunTime(milliseconds);
            if (saveChanges)
            {
                _ = service.SaveChangesAsync();
            }
        }
    }

    // 获取应用运行时间（毫秒）
    public long GetApplicationRunTime()
    {
        return ApplicationRunTime;
    }

    #endregion

    
}
