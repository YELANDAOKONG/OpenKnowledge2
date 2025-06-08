using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Utils;
using DesktopKnowledgeAvalonia.Views;
using LibraryOpenKnowledge;
using LibraryOpenKnowledge.Tools;
using Calendar = Avalonia.Controls.Calendar;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configureService;
    private readonly LocalizationService _localizationService;
    private readonly DispatcherTimer _clockTimer;
    
    [ObservableProperty]
    private string _userName;
    
    [ObservableProperty]
    private string _userInitials;
    
    [ObservableProperty]
    private string _currentDateTime;
    
    [ObservableProperty]
    private bool _isEditingUsername;

    [ObservableProperty] 
    private bool _isWindowsVisible = true;
    
    public event EventHandler? WindowCloseRequested;
    
    public string VersionInfo
    {
        get
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;

            string versionString = "Unknown";
            if (version != null)
            {
                versionString = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision:0000}";
            }

            var pVersion = DefaultClass.CurrentVersion;
            string protocolVersion = pVersion.Major + "." + pVersion.Minor + "." + pVersion.Patch;
            
            return $"OpenKnowledge Desktop {versionString} (Protocol {protocolVersion})";
        }
    }
    
    

    public MainWindowViewModel()
    {
        _configureService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        
        // Initialize user name from config
        _userName = _configureService.AppConfig.UserName;
        UpdateUserInitials();
        
        // Set up clock timer
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _clockTimer.Start();
        CurrentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    private void UpdateUserInitials()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            UserInitials = "?";
            return;
        }
        
        var parts = UserName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            UserInitials = "?";
        }
        else if (parts.Length == 1)
        {
            UserInitials = parts[0].Length > 0 ? parts[0][0].ToString().ToUpper() : "?";
        }
        else
        {
            UserInitials = (parts[0].Length > 0 ? parts[0][0].ToString() : "") + 
                          (parts[^1].Length > 0 ? parts[^1][0].ToString() : "");
            UserInitials = UserInitials.ToUpper();
        }
    }
    
    public void SaveUsername()
    {
        UpdateUserInitials();
        _configureService.AppConfig.UserName = UserName;
        _configureService.SaveChangesAsync();
    }
    
    [RelayCommand]
    private void OpenExamination()
    {
        // Simply open the examination dialog window
        var dialog = new ExaminationDialogWindow();
        IsWindowsVisible = false;
        dialog.Closed += (s, e) =>
        {
            // IsWindowsVisible = true;
            // WindowCloseRequested?.Invoke(this, EventArgs.Empty);
            MainWindow newWindows = new MainWindow();
            WindowCloseRequested?.Invoke(this, EventArgs.Empty);
            newWindows.Show();
        };
        dialog.Show();
    }
    
    [RelayCommand]
    private void OpenStudy()
    {
        StudyWindowViewModel model = new();
        StudyWindow window = new StudyWindow(model);
        IsWindowsVisible = false;
        window.Show();
        window.Closed += (s, e) => IsWindowsVisible = true;
        // TODO...
        // To be implemented
    }
    
    [RelayCommand]
    private void OpenPapers()
    {
        // TODO...
        // To be implemented
    }
    
    [RelayCommand]
    private void OpenWrongQuestions()
    {
        // TODO...
        // To be implemented
    }
    
    [RelayCommand]
    private void OpenStatistics()
    {
        // TODO...
        // To be implemented
    }
    
    [RelayCommand]
    private void OpenSettings()
    {
        SettingWindow window = new SettingWindow();
        IsWindowsVisible = false;
        window.Show();
        window.Closed += (s, e) =>
        {
            // IsWindowsVisible = true;
            MainWindow newWindows = new MainWindow();
            WindowCloseRequested?.Invoke(this, EventArgs.Empty);
            newWindows.Show();
        };
    }
    
    private readonly Random _random = new Random();
    private string _welcomeMessage;

    public string WelcomeMessage
    {
        get
        {
            if (_welcomeMessage == null)
            {
                _welcomeMessage = GetWelcomeMessage();
            }
            return _welcomeMessage;
        }
    }

    private string GetWelcomeMessage()
    {
        // 检查统计功能是否启用
        if (!_configureService.AppConfig.EnableStatistics)
        {
            return _localizationService["main.welcome"];
        }
        
        if (!_configureService.AppConfig.EnableStatistics || !_configureService.AppConfig.RandomizeWelcomeMessage)
        {
            return _localizationService["main.welcome"];
        }
        
        // 生成0-99的随机数决定显示哪种消息
        int randomValue = _random.Next(0, 100);
        
        // 15%概率显示"欢迎"
        if (randomValue < 15)
        {
            return _localizationService["main.welcome"];
        }
        
        // 20%概率显示初始化天数
        if (randomValue < 50)
        {
            // 计算初始化至今的天数
            long initTimestamp = _configureService.AppStatistics.InitializationTime;
            long currentTimestamp = TimeUtil.GetUnixTimestampMilliseconds();
            int daysSinceInit = (int)((currentTimestamp - initTimestamp) / (1000 * 60 * 60 * 24));
            
            return string.Format(_localizationService["main.stats.days.since.init"], daysSinceInit);
        }
        
        // 剩余50%概率显示统计信息
        
        // 确定时间段（周、月、年、总计）
        int timePeriodValue = _random.Next(0, 100);
        string timePeriod;
        
        if (timePeriodValue < 40)  // 40%概率显示本周
        {
            timePeriod = "week";
        }
        else if (timePeriodValue < 70)  // 30%概率显示本月
        {
            timePeriod = "month";
        }
        else if (timePeriodValue < 90)  // 20%概率显示今年
        {
            timePeriod = "year";
        }
        else  // 10%概率显示总计
        {
            timePeriod = "total";
        }
        
        // 随机选择4种统计类型之一
        int statTypeValue = _random.Next(0, 4);
        
        int statValue = 0;
        string statKey = "";
        
        DateTime now = DateTime.UtcNow;
        int currentYear = now.Year;
        int currentMonth = now.Month;
        
        System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        int currentWeek = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        
        switch (statTypeValue)
        {
            case 0: // AI调用次数
                statKey = $"main.stats.{timePeriod}.ai.calls";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.AiCallCountWeeks.TryGetValue(currentYear, out var yearDict) && 
                                yearDict.TryGetValue(currentWeek, out var count) ? count : 0;
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.AiCallCountMonths.TryGetValue(currentYear, out var yearDict) && 
                                yearDict.TryGetValue(currentMonth, out var count) ? count : 0;
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.AiCallCountYears.TryGetValue(currentYear, out var count) ? count : 0;
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.AiCallCount;
                }
                break;
                
            case 1: // 加载考试次数
                statKey = $"main.stats.{timePeriod}.exams.loaded";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.LoadExaminationCountWeeks.TryGetValue(currentYear, out var yearDict) && 
                                yearDict.TryGetValue(currentWeek, out var count) ? count : 0;
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.LoadExaminationCountMonths.TryGetValue(currentYear, out var yearDict) && 
                                yearDict.TryGetValue(currentMonth, out var count) ? count : 0;
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.LoadExaminationCountYears.TryGetValue(currentYear, out var count) ? count : 0;
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.LoadExaminationCount;
                }
                break;
                
            case 2: // 提交考试次数
                statKey = $"main.stats.{timePeriod}.exams.submitted";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.SubmitExaminationCountWeeks.TryGetValue(currentYear, out var yearDict) && 
                               yearDict.TryGetValue(currentWeek, out var count) ? count : 0;
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.SubmitExaminationCountMonths.TryGetValue(currentYear, out var yearDict) && 
                               yearDict.TryGetValue(currentMonth, out var count) ? count : 0;
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.SubmitExaminationCountYears.TryGetValue(currentYear, out var count) ? count : 0;
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.SubmitExaminationCount;
                }
                break;
                
            case 3: // 程序启动次数
            default:
                statKey = $"main.stats.{timePeriod}.app.starts";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.ApplicationStartCountWeeks.TryGetValue(currentYear, out var yearDict) && 
                               yearDict.TryGetValue(currentWeek, out var count) ? count : 0;
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.ApplicationStartCountMonths.TryGetValue(currentYear, out var yearDict) && 
                               yearDict.TryGetValue(currentMonth, out var count) ? count : 0;
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.ApplicationStartCountYears.TryGetValue(currentYear, out var count) ? count : 0;
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.ApplicationStartCount;
                }
                break;
        }
        
        return string.Format(_localizationService[statKey], statValue);
    }
    

}
