using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledge.Services;
using DesktopKnowledge.Models;

namespace DesktopKnowledge.ViewModels;

public partial class StatisticsWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    public event EventHandler? WindowCloseRequested;

    [ObservableProperty]
    private ObservableCollection<StatisticsCategory> _categories = new();

    [ObservableProperty]
    private StatisticsCategory? _selectedCategory;
    
    private readonly LoggerService _logger;

    public StatisticsWindowViewModel()
    {
        _configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _logger = App.GetWindowsLogger("StatisticsWindow");

        InitializeCategories();
    }

    private void InitializeCategories()
    {
        // Overview Statistics
        var overview = new StatisticsCategory(
            _localizationService["statistics.category.overview"],
            "M16 6l2.29 2.29-4.88 4.88-4-4L2 16.59 3.41 18l6-6 4 4 6.3-6.29L22 12V6z", // Chart icon
            new OverviewStatisticsViewModel(_configService, _localizationService));

        // Daily Statistics
        var daily = new StatisticsCategory(
            _localizationService["statistics.category.daily"],
            "M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z", // Calendar day icon
            new DailyStatisticsViewModel(_configService, _localizationService));

        // Weekly Statistics
        var weekly = new StatisticsCategory(
            _localizationService["statistics.category.weekly"],
            "M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 14H7v-5h5v5zm5-9H7V5h10v3z", // Calendar week icon
            new WeeklyStatisticsViewModel(_configService, _localizationService));

        // Monthly Statistics
        var monthly = new StatisticsCategory(
            _localizationService["statistics.category.monthly"],
            "M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11zM7 11h2v2H7zm4 0h2v2h-2zm4 0h2v2h-2z", // Calendar month icon
            new MonthlyStatisticsViewModel(_configService, _localizationService));

        // Yearly Statistics
        var yearly = new StatisticsCategory(
            _localizationService["statistics.category.yearly"],
            "M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11z", // Calendar year icon
            new YearlyStatisticsViewModel(_configService, _localizationService));

        // Time Statistics
        var time = new StatisticsCategory(
            _localizationService["statistics.category.time"],
            "M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z", // Clock icon
            new TimeStatisticsViewModel(_configService, _localizationService));

        Categories.Add(overview);
        Categories.Add(daily);
        Categories.Add(weekly);
        Categories.Add(monthly);
        Categories.Add(yearly);
        Categories.Add(time);

        SelectedCategory = overview;
    }

    [RelayCommand]
    private void Exit()
    {
        WindowCloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

public class StatisticsCategory
{
    public string DisplayName { get; }
    public string IconPath { get; }
    public object Content { get; }

    public StatisticsCategory(string displayName, string iconPath, object content)
    {
        DisplayName = displayName;
        IconPath = iconPath;
        Content = content;
    }
}

// Base class for all statistics views
public abstract partial class StatisticsViewModelBase : ViewModelBase
{
    protected readonly ConfigureService ConfigService;
    protected readonly LocalizationService LocalizationService;

    protected StatisticsViewModelBase(ConfigureService configService, LocalizationService localizationService)
    {
        ConfigService = configService;
        LocalizationService = localizationService;
    }
    
    [RelayCommand]
    public virtual void Update()
    {
        Console.WriteLine("Update command executed");
        try
        {
            LoadStatistics();
            Console.WriteLine("Statistics loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Update command: {ex}");
        }
    }
    
    protected abstract void LoadStatistics();
}

// Overview Statistics View
public partial class OverviewStatisticsViewModel : StatisticsViewModelBase
{
    // Total counts
    [ObservableProperty] private int _totalAppStarts;
    [ObservableProperty] private int _totalAiCalls;
    [ObservableProperty] private int _totalQuestionInteractions;
    [ObservableProperty] private int _totalLoadExaminations;
    [ObservableProperty] private int _totalSubmitExaminations;
    [ObservableProperty] private int _totalStartStudy;
    [ObservableProperty] private int _totalCompleteStudy;
    
    // Time statistics
    [ObservableProperty] private string _totalExaminationTime;
    [ObservableProperty] private string _totalStudyTime;
    [ObservableProperty] private string _totalAppRunTime;
    
    // Initialization date
    [ObservableProperty] private string _initializationDate;
    [ObservableProperty] private string _daysSinceInit;

    public OverviewStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
        
        // Total counts
        TotalAppStarts = stats.GetApplicationStartCount();
        TotalAiCalls = stats.GetAiCallCount();
        TotalQuestionInteractions = stats.GetQuestionInteractionCount();
        TotalLoadExaminations = stats.GetLoadExaminationCount();
        TotalSubmitExaminations = stats.GetSubmitExaminationCount();
        TotalStartStudy = stats.GetStartStudyCount();
        TotalCompleteStudy = stats.GetCompleteStudyCount();
        
        // Time statistics
        TotalExaminationTime = FormatTimeSpan(TimeSpan.FromMilliseconds(stats.GetExaminationTime()));
        TotalStudyTime = FormatTimeSpan(TimeSpan.FromMilliseconds(stats.GetStudyTime()));
        TotalAppRunTime = FormatTimeSpan(TimeSpan.FromMilliseconds(stats.GetApplicationRunTime()));
        
        // Initialization date
        var initDate = DateTimeOffset.FromUnixTimeMilliseconds(stats.InitializationTime).ToLocalTime().DateTime;
        InitializationDate = initDate.ToString("yyyy-MM-dd");
        
        var daysSince = (DateTime.Now - initDate).Days;
        DaysSinceInit = daysSince.ToString();
    }
    
    private string FormatTimeSpan(TimeSpan time)
    {
        if (time.TotalDays >= 1)
        {
            return $"{time.Days}d {time.Hours}h {time.Minutes}m";
        }
        else if (time.TotalHours >= 1)
        {
            return $"{time.Hours}h {time.Minutes}m {time.Seconds}s";
        }
        else
        {
            return $"{time.Minutes}m {time.Seconds}s";
        }
    }
}

// Daily Statistics View
public partial class DailyStatisticsViewModel : StatisticsViewModelBase
{
    [ObservableProperty] private DateTimeOffset? _selectedDate;
    [ObservableProperty] private ObservableCollection<DailyStatItem> _dailyStats = new();

    public DailyStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        SelectedDate = DateTimeOffset.Now;
        LoadStatistics();
    }

    [RelayCommand]
    private void DateChanged()
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
        
        // 获取用户选择的本地时间
        DateTime localDate = SelectedDate?.DateTime ?? DateTime.Today;
        
        // 将本地时间转换为UTC时间用于查询
        DateTime utcDate = localDate.ToUniversalTime();
        
        int year = utcDate.Year;
        int month = utcDate.Month;
        int day = utcDate.Day;
        
        // 创建新的集合实例
        var newStats = new ObservableCollection<DailyStatItem>();
        
        // 查询UTC时间对应的统计数据
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.app.starts"], 
            Count = stats.GetApplicationStartCountDay(year, month, day) 
        });
        
        // 其他统计项保持相同模式...
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.ai.calls"], 
            Count = stats.GetAiCallCountDay(year, month, day)
        });
        
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.question.interactions"], 
            Count = stats.GetQuestionInteractionCountDay(year, month, day)
        });
        
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.loaded"], 
            Count = stats.GetLoadExaminationCountDay(year, month, day)
        });
        
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.submitted"], 
            Count = stats.GetSubmitExaminationCountDay(year, month, day)
        });
        
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.study.started"], 
            Count = stats.GetStartStudyCountDay(year, month, day)
        });
        
        newStats.Add(new DailyStatItem 
        { 
            StatName = LocalizationService["statistics.study.completed"], 
            Count = stats.GetCompleteStudyCountDay(year, month, day)
        });
        
        // 替换整个集合
        DailyStats = newStats;
    }
}

public class DailyStatItem
{
    public string StatName { get; set; } = "";
    public int Count { get; set; }
}

// Weekly Statistics View
public partial class WeeklyStatisticsViewModel : StatisticsViewModelBase
{
    [ObservableProperty] private int _selectedYear;
    [ObservableProperty] private int _selectedWeek;
    [ObservableProperty] private List<int> _availableYears = new();
    [ObservableProperty] private List<int> _availableWeeks = new();
    [ObservableProperty] private ObservableCollection<WeeklyStatItem> _weeklyStats = new();

    public WeeklyStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        InitializeYearsAndWeeks();
        LoadStatistics();
    }

    private void InitializeYearsAndWeeks()
    {
        int currentYear = DateTime.Now.Year;
        // 扩展年份范围：从2015年到当前年份后5年
        // AvailableYears = Enumerable.Range(2015, currentYear - 2015 + 6).ToList();
        AvailableYears = Enumerable.Range(currentYear - 5, 11).ToList();
        
        SelectedYear = currentYear;
        
        // Weeks 1-53
        AvailableWeeks = Enumerable.Range(1, 53).ToList();
        
        // Get current week
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        SelectedWeek = calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    [RelayCommand]
    private void YearOrWeekChanged()
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
        
        // 从本地时间年份和周转换为UTC时间范围
        // 获取选定年份和周的日期范围
        DateTime firstDayOfWeek = GetFirstDayOfWeek(SelectedYear, SelectedWeek);
        
        // 将本地时间转换为UTC
        DateTime utcFirstDayOfWeek = firstDayOfWeek.ToUniversalTime();
        int utcYear = utcFirstDayOfWeek.Year;
        int utcWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            utcFirstDayOfWeek, 
            CalendarWeekRule.FirstFourDayWeek, 
            DayOfWeek.Monday);
        
        // 创建新的集合实例
        var newStats = new ObservableCollection<WeeklyStatItem>();
        
        // 使用UTC年份和周查询统计数据
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.app.starts"], 
            Count = stats.GetApplicationStartCountWeek(utcYear, utcWeek) 
        });
        
        // 其他统计项保持相同模式...
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.ai.calls"], 
            Count = stats.GetAiCallCountWeek(utcYear, utcWeek)
        });
        
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.question.interactions"], 
            Count = stats.GetQuestionInteractionCountWeek(utcYear, utcWeek)
        });
        
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.loaded"], 
            Count = stats.GetLoadExaminationCountWeek(utcYear, utcWeek)
        });
        
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.submitted"], 
            Count = stats.GetSubmitExaminationCountWeek(utcYear, utcWeek)
        });
        
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.study.started"], 
            Count = stats.GetStartStudyCountWeek(utcYear, utcWeek)
        });
        
        newStats.Add(new WeeklyStatItem 
        { 
            StatName = LocalizationService["statistics.study.completed"], 
            Count = stats.GetCompleteStudyCountWeek(utcYear, utcWeek)
        });
        
        // 替换整个集合
        WeeklyStats = newStats;
    }
    
    // 辅助方法：获取指定年份和周的第一天
    private DateTime GetFirstDayOfWeek(int year, int weekOfYear)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        
        if (daysOffset > 0) daysOffset -= 7;
        
        DateTime firstMonday = jan1.AddDays(daysOffset);
        
        // 周数从1开始，所以需要减1再乘以7天
        int firstDayOfWeek = (weekOfYear - 1) * 7;
        
        return firstMonday.AddDays(firstDayOfWeek);
    }
}

public class WeeklyStatItem
{
    public string StatName { get; set; } = "";
    public int Count { get; set; }
}

// Monthly Statistics View
public partial class MonthlyStatisticsViewModel : StatisticsViewModelBase
{
    [ObservableProperty] private int _selectedYear;
    [ObservableProperty] private int _selectedMonth;
    [ObservableProperty] private List<int> _availableYears = new();
    [ObservableProperty] private List<MonthOption> _availableMonths = new();
    [ObservableProperty] private ObservableCollection<MonthlyStatItem> _monthlyStats = new();

    public MonthlyStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        InitializeYearsAndMonths();
        LoadStatistics();
    }

    private void InitializeYearsAndMonths()
    {
        int currentYear = DateTime.Now.Year;
        // 扩展年份范围：从2015年到当前年份后5年
        // AvailableYears = Enumerable.Range(2015, currentYear - 2015 + 6).ToList();
        AvailableYears = Enumerable.Range(currentYear - 5, 11).ToList();
        
        SelectedYear = currentYear;
        
        AvailableMonths = new List<MonthOption>
        {
            new() { Number = 1, Name = LocalizationService["month.january"] },
            new() { Number = 2, Name = LocalizationService["month.february"] },
            new() { Number = 3, Name = LocalizationService["month.march"] },
            new() { Number = 4, Name = LocalizationService["month.april"] },
            new() { Number = 5, Name = LocalizationService["month.may"] },
            new() { Number = 6, Name = LocalizationService["month.june"] },
            new() { Number = 7, Name = LocalizationService["month.july"] },
            new() { Number = 8, Name = LocalizationService["month.august"] },
            new() { Number = 9, Name = LocalizationService["month.september"] },
            new() { Number = 10, Name = LocalizationService["month.october"] },
            new() { Number = 11, Name = LocalizationService["month.november"] },
            new() { Number = 12, Name = LocalizationService["month.december"] }
        };
        
        SelectedMonth = DateTime.Now.Month;
    }

    [RelayCommand]
    private void YearOrMonthChanged()
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
        int year = SelectedYear;
        int month = SelectedMonth;
    
        // 创建新的集合实例
        var newStats = new ObservableCollection<MonthlyStatItem>();
    
        // 使用UTC年份和月份查询统计数据
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.app.starts"],    
            Count = stats.GetApplicationStartCountMonth(year, month) 
        });
    
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.ai.calls"], 
            Count = stats.GetAiCallCountMonth(year, month)
        });
    
        // 其他统计项同样需要修改...
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.question.interactions"], 
            Count = stats.GetQuestionInteractionCountMonth(year, month)
        });
    
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.loaded"], 
            Count = stats.GetLoadExaminationCountMonth(year, month)
        });
    
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.submitted"], 
            Count = stats.GetSubmitExaminationCountMonth(year, month)
        });
    
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.study.started"], 
            Count = stats.GetStartStudyCountMonth(year, month)
        });
    
        newStats.Add(new MonthlyStatItem 
        { 
            StatName = LocalizationService["statistics.study.completed"], 
            Count = stats.GetCompleteStudyCountMonth(year, month)
        });
    
        // 替换整个集合
        MonthlyStats = newStats;
    }
}

public class MonthOption
{
    public int Number { get; set; }
    public string Name { get; set; } = "";
}

public class MonthlyStatItem
{
    public string StatName { get; set; } = "";
    public int Count { get; set; }
}

// Yearly Statistics View
public partial class YearlyStatisticsViewModel : StatisticsViewModelBase
{
    [ObservableProperty] private int _selectedYear;
    [ObservableProperty] private List<int> _availableYears = new();
    [ObservableProperty] private ObservableCollection<YearlyStatItem> _yearlyStats = new();

    public YearlyStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        InitializeYears();
        LoadStatistics();
    }

    private void InitializeYears()
    {
        int currentYear = DateTime.Now.Year;
        // 扩展年份范围：从2015年到当前年份后5年
        // AvailableYears = Enumerable.Range(2015, currentYear - 2015 + 6).ToList();
        AvailableYears = Enumerable.Range(currentYear - 5, 11).ToList();
        
        SelectedYear = currentYear;
    }

    [RelayCommand]
    private void YearChanged()
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
        int year = SelectedYear;
        
        // 创建新的集合实例
        var newStats = new ObservableCollection<YearlyStatItem>();
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.app.starts"], 
            Count = stats.GetApplicationStartCountYear(year) 
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.ai.calls"], 
            Count = stats.GetAiCallCountYear(year)
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.question.interactions"], 
            Count = stats.GetQuestionInteractionCountYear(year)
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.loaded"], 
            Count = stats.GetLoadExaminationCountYear(year)
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.exams.submitted"], 
            Count = stats.GetSubmitExaminationCountYear(year)
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.study.started"], 
            Count = stats.GetStartStudyCountYear(year)
        });
        
        newStats.Add(new YearlyStatItem 
        { 
            StatName = LocalizationService["statistics.study.completed"], 
            Count = stats.GetCompleteStudyCountYear(year)
        });
        
        // 替换整个集合
        YearlyStats = newStats;
    }
}

public class YearlyStatItem
{
    public string StatName { get; set; } = "";
    public int Count { get; set; }
}

// Time Statistics View
public partial class TimeStatisticsViewModel : StatisticsViewModelBase
{
    [ObservableProperty] private string _totalExaminationTime;
    [ObservableProperty] private string _totalStudyTime;
    [ObservableProperty] private string _totalAppRunTime;
    [ObservableProperty] private double _examPercentage;
    [ObservableProperty] private double _studyPercentage;
    [ObservableProperty] private double _otherPercentage;

    public TimeStatisticsViewModel(ConfigureService configService, LocalizationService localizationService) 
        : base(configService, localizationService)
    {
        LoadStatistics();
    }

    protected override void LoadStatistics()
    {
        var stats = ConfigService.AppStatistics;
    
        // 时间统计（毫秒）
        long examTimeMs = stats.GetExaminationTime();
        long studyTimeMs = stats.GetStudyTime();
        long appRunTimeMs = stats.GetApplicationRunTime();
    
        // 转换为小时显示
        double examTimeHours = examTimeMs / 3600000.0;
        double studyTimeHours = studyTimeMs / 3600000.0;
        double appRunTimeHours = appRunTimeMs / 3600000.0;
    
        TotalExaminationTime = $"{examTimeHours:F1} {LocalizationService["time.hours"]}";
        TotalStudyTime = $"{studyTimeHours:F1} {LocalizationService["time.hours"]}";
        TotalAppRunTime = $"{appRunTimeHours:F1} {LocalizationService["time.hours"]}";
    
        // 计算百分比用于可视化
        if (appRunTimeMs > 0)
        {
            // 计算原始百分比
            double examPercent = (double)examTimeMs / appRunTimeMs * 100;
            double studyPercent = (double)studyTimeMs / appRunTimeMs * 100;
        
            // 确保非零值有最小可见百分比
            if (examTimeMs > 0 && examPercent < 0.1) examPercent = 0.1;
            if (studyTimeMs > 0 && studyPercent < 0.1) studyPercent = 0.1;
        
            // 设置最终值
            ExamPercentage = Math.Round(examPercent, 1);
            StudyPercentage = Math.Round(studyPercent, 1);
            OtherPercentage = Math.Round(100 - ExamPercentage - StudyPercentage, 1);
        
            // 确保没有负值
            if (OtherPercentage < 0) OtherPercentage = 0;
        }
        else
        {
            // 如果没有应用运行时间，设置默认值
            ExamPercentage = 0;
            StudyPercentage = 0;
            OtherPercentage = 100;
        }
    }
}
