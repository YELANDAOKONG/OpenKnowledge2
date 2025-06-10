using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
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
    
    [ObservableProperty]
    private Bitmap? _avatarImage;
    
    [ObservableProperty]
    private bool _hasCustomAvatar;
    
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
        
        // Load avatar if available
        LoadAvatarAsync().ConfigureAwait(false);
        
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
    
    private async Task LoadAvatarAsync()
    {
        var avatarPath = _configureService.AppConfig.AvatarFilePath;
        
        if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
        {
            try
            {
                await using var stream = File.OpenRead(avatarPath);
                AvatarImage = new Bitmap(stream);
                HasCustomAvatar = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading avatar: {ex.Message}");
                AvatarImage = null;
                HasCustomAvatar = false;
            }
        }
        else
        {
            AvatarImage = null;
            HasCustomAvatar = false;
        }
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
        StatisticsWindow window = new StatisticsWindow();
        IsWindowsVisible = false;
        window.Show();
        window.Closed += (s, e) => IsWindowsVisible = true;
    }
    
    [RelayCommand]
    private void OpenTools()
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
        if (randomValue < 35)
        {
            // 计算初始化至今的天数
            long initTimestamp = _configureService.AppStatistics.InitializationTime;
            long currentTimestamp = TimeUtil.GetUnixTimestampMilliseconds();
            int daysSinceInit = (int)((currentTimestamp - initTimestamp) / (1000 * 60 * 60 * 24));
            
            return string.Format(_localizationService["main.stats.days.since.init"], daysSinceInit);
        }
        
        // 剩余65%概率显示统计信息
        
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
        
        // 随机选择统计类型
        // 除了总计情况下可能显示学习和考试时间外，其他情况下显示8种统计类型之一
        int statTypeCount = (timePeriod == "total") ? 10 : 8;
        int statTypeValue = _random.Next(0, statTypeCount);
        
        object statValue = 0;
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
                    statValue = _configureService.AppStatistics.GetAiCallCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetAiCallCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetAiCallCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetAiCallCount();
                }
                break;
                
            case 1: // 加载考试次数
                statKey = $"main.stats.{timePeriod}.exams.loaded";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetLoadExaminationCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetLoadExaminationCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetLoadExaminationCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetLoadExaminationCount();
                }
                break;
                
            case 2: // 提交考试次数
                statKey = $"main.stats.{timePeriod}.exams.submitted";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetSubmitExaminationCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetSubmitExaminationCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetSubmitExaminationCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetSubmitExaminationCount();
                }
                break;
                
            case 3: // 程序启动次数
                statKey = $"main.stats.{timePeriod}.app.starts";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCount();
                }
                break;
                
            case 4: // 题目交互次数
                statKey = $"main.stats.{timePeriod}.question.interactions";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetQuestionInteractionCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetQuestionInteractionCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetQuestionInteractionCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetQuestionInteractionCount();
                }
                break;
                
            case 5: // 启动学习次数
                statKey = $"main.stats.{timePeriod}.study.started";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetStartStudyCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetStartStudyCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetStartStudyCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetStartStudyCount();
                }
                break;
                
            case 6: // 完成学习次数
                statKey = $"main.stats.{timePeriod}.study.completed";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetCompleteStudyCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetCompleteStudyCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetCompleteStudyCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetCompleteStudyCount();
                }
                break;
                
            case 7: // 其他可能的统计项（可以根据需要添加）
                // 回到默认情况，显示程序启动次数
                statKey = $"main.stats.{timePeriod}.app.starts";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCount();
                }
                break;
                
            case 8: // 考试时间（仅在total中显示）
                statKey = "main.stats.total.examination.time";
                statValue = Math.Round(_configureService.AppStatistics.GetExaminationTime() / 3600000.0, 1); // 转换为小时，保留一位小数
                break;
                
            case 9: // 学习时间（仅在total中显示）
                statKey = "main.stats.total.study.time";
                statValue = Math.Round(_configureService.AppStatistics.GetStudyTime() / 3600000.0, 1); // 转换为小时，保留一位小数
                break;
                
            default: // 默认回到程序启动次数
                statKey = $"main.stats.{timePeriod}.app.starts";
                if (timePeriod == "week")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountWeek();
                }
                else if (timePeriod == "month")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountMonth();
                }
                else if (timePeriod == "year")
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCountYear();
                }
                else // total
                {
                    statValue = _configureService.AppStatistics.GetApplicationStartCount();
                }
                break;
        }
        
        return string.Format(_localizationService[statKey], statValue);
    }
    

}
