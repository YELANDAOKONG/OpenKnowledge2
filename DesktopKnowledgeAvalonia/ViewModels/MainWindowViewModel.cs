using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Views;
using LibraryOpenKnowledge;
using LibraryOpenKnowledge.Tools;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configureService;
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
    private async Task OpenExamination()
    {
        // Create OpenFileDialog to select an examination file
        var dialog = new OpenFileDialog
        {
            Title = "Select File",
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "Exam", Extensions = new List<string> { "json" } },
                new FileDialogFilter { Name = "All", Extensions = new List<string> { "*" } }
            },
            AllowMultiple = false
        };

        Window? currentWindow = null;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            currentWindow = desktop.MainWindow;
        }
        // Show the dialog and get the result
        string[] result = await dialog.ShowAsync(currentWindow!);
    
        // Get the selected file path or null if cancelled
        string? filePath = result?.Length > 0 ? result[0] : null;

        // Create and show the examination window
        ExaminationWindow window = new ExaminationWindow(filePath, filePath != null);
        IsWindowsVisible = false;
        window.Show();
        window.Closed += (s, e) => IsWindowsVisible = true;
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
}
