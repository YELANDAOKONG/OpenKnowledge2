namespace DesktopKnowledgeAvalonia.ViewModels;

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Services;
using LibraryOpenKnowledge.Tools;

public partial class ExaminationDialogWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    private readonly DispatcherTimer _statusMessageTimer;
    
    [ObservableProperty]
    private string _examStatusTitle;
    
    [ObservableProperty]
    private string _truncatedExamTitle;
    
    [ObservableProperty]
    private bool _hasActiveExam;
    
    [ObservableProperty]
    private string? _examId;
    
    [ObservableProperty]
    private string? _examTitle;
    
    [ObservableProperty]
    private string? _examDescription;
    
    [ObservableProperty]
    private string? _examSubject;
    
    [ObservableProperty]
    private string? _examLanguage;
    
    [ObservableProperty]
    private string? _examTotalScore;
    
    [ObservableProperty]
    private string _statusMessage = "";
    
    [ObservableProperty]
    private bool _showStatusMessage;
    
    [ObservableProperty]
    private string _continueButtonText;
    
    public bool IsExamLoadedButNotStarted => 
        _hasActiveExam && !_configService.AppData.IsTheExaminationStarted;

    [ObservableProperty] 
    private bool _isWindowsVisible = true;

    public event EventHandler? CloseRequested;
    public event EventHandler? ContinueExamRequested;
    public event EventHandler? LoadNewExamRequested;
    public event EventHandler? DeleteCurrentExamRequested;
    
    public event EventHandler? WindowCloseRequested;
    
    public ExaminationDialogWindowViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _isWindowsVisible = true;
        
        // Setup status message timer
        _statusMessageTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _statusMessageTimer.Tick += (s, e) => 
        {
            ShowStatusMessage = false;
            _statusMessageTimer.Stop();
        };
        
        // Initialize properties based on current examination state
        UpdateExamInfo();
    }
    
    private void UpdateExamInfo()
    {
        HasActiveExam = _configService.AppData.IsInExamination && _configService.AppData.CurrentExamination != null;
        
        if (HasActiveExam && _configService.AppData.CurrentExamination != null)
        {
            var exam = _configService.AppData.CurrentExamination;
            
            // Set the appropriate status title and button text based on whether the exam has been started
            if (_configService.AppData.IsTheExaminationStarted)
            {
                ExamStatusTitle = _localizationService["exam.dialog.status.ongoing"];
                ContinueButtonText = _localizationService["exam.dialog.continue"];
            }
            else
            {
                ExamStatusTitle = _localizationService["exam.dialog.status.loaded"];
                ContinueButtonText = _localizationService["exam.dialog.start"];
            }
            
            // Set examination properties
            ExamId = exam.ExaminationMetadata.ExamId;
            ExamTitle = exam.ExaminationMetadata.Title;
            TruncatedExamTitle = TruncateString(ExamTitle, 64);
            ExamDescription = exam.ExaminationMetadata.Description;
            ExamSubject = exam.ExaminationMetadata.Subject;
            ExamLanguage = exam.ExaminationMetadata.Language;
            ExamTotalScore = exam.ExaminationMetadata.TotalScore.ToString();
        }
        else
        {
            ExamStatusTitle = _localizationService["exam.dialog.status.none"];
            ContinueButtonText = _localizationService["exam.dialog.continue"];
            TruncatedExamTitle = "";
            
            // Clear examination properties
            ExamId = null;
            ExamTitle = null;
            ExamDescription = null;
            ExamSubject = null;
            ExamLanguage = null;
            ExamTotalScore = null;
        }
    }
    
    private string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;
            
        return input.Substring(0, maxLength - 3) + "...";
    }
    
    private void ShowTemporaryStatusMessage(string message)
    {
        StatusMessage = message;
        ShowStatusMessage = true;
        _statusMessageTimer.Stop();
        _statusMessageTimer.Start();
    }
    
    [RelayCommand]
    private void ContinueExam()
    {
        if (!HasActiveExam)
            return;
            
        ContinueExamRequested?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private async Task LoadNewExamAsync()
    {
        LoadNewExamRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void LoadExamination(string filePath)
    {
        try
        {
            // Deserialize the examination from the file
            var examination = ExaminationSerializer.DeserializeFromFile(filePath);
            
            if (examination != null)
            {
                // Set the current examination in the app data
                _configService.AppData.CurrentExamination = examination;
                _configService.AppData.IsInExamination = true;
                _configService.AppData.IsTheExaminationStarted = false; // Mark as loaded but not started
                
                // Save changes to config
                _configService.SaveChangesAsync();
                
                // Update the UI
                UpdateExamInfo();
                
                // Show success message
                ShowTemporaryStatusMessage(_localizationService["exam.dialog.load.success"]);
            }
            else
            {
                // Show error message
                ShowTemporaryStatusMessage(_localizationService["exam.dialog.load.error"]);
            }
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error loading examination: {ex.Message}");
            
            // Show error message
            ShowTemporaryStatusMessage(_localizationService["exam.dialog.load.error"]);
        }
    }
    
    public void MarkExamAsStarted()
    {
        if (!HasActiveExam)
            return;
            
        // Mark the examination as started
        _configService.AppData.IsTheExaminationStarted = true;
        
        // Save changes to config
        _configService.SaveChangesAsync();
    }
    
    [RelayCommand]
    private async Task SaveCurrentExamAsync()
    {
        if (!HasActiveExam || _configService.AppData.CurrentExamination == null)
            return;
            
        try
        {
            // Create save file picker options
            var options = new FilePickerSaveOptions
            {
                Title = _localizationService["exam.dialog.save.title"],
                SuggestedFileName = $"exam_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                DefaultExtension = "json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" },
                        MimeTypes = new[] { "*/*" }
                    }
                }
            };
            
            // Get the parent window
            var window = (Window)CloseRequested?.Target!;
            if (window == null) return;
            
            // Show the save file picker
            var result = await window.StorageProvider.SaveFilePickerAsync(options);
            
            if (result != null)
            {
                // Get the file path
                var filePath = result.Path.LocalPath;
                
                // Serialize the examination to the selected file
                bool success = ExaminationSerializer.SerializeToFile(
                    _configService.AppData.CurrentExamination,
                    filePath,
                    includeUserAnswers: true);
                    
                if (success)
                {
                    // Show success message
                    ShowTemporaryStatusMessage(_localizationService["exam.dialog.save.success"]);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error saving examination: {ex.Message}");
            
            // Show error message
            ShowTemporaryStatusMessage(_localizationService["exam.dialog.save.error"]);
        }
    }
    
    [RelayCommand]
    private void DeleteCurrentExam()
    {
        if (!HasActiveExam)
            return;
            
        DeleteCurrentExamRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void ConfirmDeleteCurrentExam()
    {
        if (!HasActiveExam)
            return;
            
        // Clear the current examination
        _configService.AppData.CurrentExamination = null;
        _configService.AppData.IsInExamination = false;
        _configService.AppData.IsTheExaminationStarted = false;
        _configService.AppData.ExaminationTimer = null;
        
        // Save changes to config
        _configService.SaveChangesAsync();
        
        // Update the UI
        UpdateExamInfo();
        
        // Show success message
        ShowTemporaryStatusMessage(_localizationService["exam.dialog.delete.success"]);
    }
}
