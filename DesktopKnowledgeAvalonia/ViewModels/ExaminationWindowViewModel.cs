using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Views;
using LibraryOpenKnowledge.Extensions;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class ExaminationWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private Examination? _examination;
    
    [ObservableProperty]
    private ExaminationSection? _currentSection;
    
    [ObservableProperty]
    private Question? _currentQuestion;
    
    [ObservableProperty]
    private int _currentSectionIndex;
    
    [ObservableProperty]
    private int _currentQuestionIndex;
    
    [ObservableProperty]
    private string _timeRemaining = "00:00:00";
    
    [ObservableProperty]
    private double _progressPercentage;
    
    [ObservableProperty] 
    private bool _isSaving;
    
    [ObservableProperty]
    private string? _statusMessage;
    
    [ObservableProperty]
    private bool _showStatusMessage;

    [ObservableProperty] 
    private bool _isWindowVisible;
    
    // Event handlers to notify the view of changes
    public event EventHandler? ExaminationLoaded;
    public event EventHandler? QuestionChanged;
    public event EventHandler? ProgressUpdated;
    
    public event EventHandler? WindowCloseRequested;
    
    // This will be assigned by the view
    public Action SaveCurrentAnswer { get; set; } = () => { };
    
    public ExaminationWindowViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _isWindowVisible = true;
    }
    
    public void Initialize()
    {
        // Initialize with current examination from configuration
        if (_configService.AppData.CurrentExamination != null)
        {
            Examination = _configService.AppData.CurrentExamination;
            EnsureJudgmentQuestionOptions();
            InitializeExamination();
        }
    }
    
    private void InitializeExamination()
    {
        if (Examination == null) return;
        
        // Set initial section and question
        if (Examination.ExaminationSections.Length > 0)
        {
            CurrentSectionIndex = 0;
            CurrentSection = Examination.ExaminationSections[0];
            
            if (CurrentSection.Questions?.Length > 0)
            {
                CurrentQuestionIndex = 0;
                CurrentQuestion = CurrentSection.Questions[0];
            }
        }
        
        UpdateProgress();
        ExaminationLoaded?.Invoke(this, EventArgs.Empty);
    }
    
    private void StartTimer()
    {
        // Set up a timer to update the time remaining
        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (sender, args) =>
        {
            if (_configService.AppData.ExaminationTimer.HasValue)
            {
                var elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _configService.AppData.ExaminationTimer.Value;
                var timeSpan = TimeSpan.FromMilliseconds(elapsed);
                TimeRemaining = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
        };
        timer.Start();
    }
    
    public void UpdateProgress()
    {
        if (Examination == null) return;
        
        // Calculate progress based on answered questions
        int totalQuestions = 0;
        int answeredQuestions = 0;
        
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            totalQuestions += section.Questions.Length;
            answeredQuestions += section.Questions.Count(q => q.UserAnswer != null && q.UserAnswer.Length > 0);
        }
        
        ProgressPercentage = totalQuestions > 0 
            ? (double)answeredQuestions / totalQuestions * 100 
            : 0;
        
        ProgressUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void EnsureJudgmentQuestionOptions()
    {
        // Go through all sections and questions to ensure judgment questions have proper options
        if (Examination == null) return;
    
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
        
            foreach (var question in section.Questions)
            {
                if (question.Type == QuestionTypes.Judgment)
                {
                    // Ensure options exist for judgment questions
                    if (question.Options == null || question.Options.Count < 2)
                    {
                        question.Options = new List<(string, string)>
                        {
                            ("True", "True"),
                            ("False", "False")
                        }.ToOptionList();
                    }
                }
            }
        }
    }
    
    public void NavigateToQuestion(int sectionIndex, int questionIndex)
    {
        if (Examination == null) return;
        
        // Save current answer before switching
        SaveCurrentAnswer();
        
        if (sectionIndex >= 0 && sectionIndex < Examination.ExaminationSections.Length)
        {
            CurrentSectionIndex = sectionIndex;
            CurrentSection = Examination.ExaminationSections[sectionIndex];
            
            if (CurrentSection.Questions != null && 
                questionIndex >= 0 && 
                questionIndex < CurrentSection.Questions.Length)
            {
                CurrentQuestionIndex = questionIndex;
                CurrentQuestion = CurrentSection.Questions[questionIndex];
                QuestionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public void NavigatePrevious()
    {
        if (Examination == null || CurrentSection == null) return;
        
        // Save current answer before navigating
        SaveCurrentAnswer();
        
        // Check if we can navigate to previous question in current section
        if (CurrentQuestionIndex > 0)
        {
            CurrentQuestionIndex--;
            CurrentQuestion = CurrentSection.Questions![CurrentQuestionIndex];
            QuestionChanged?.Invoke(this, EventArgs.Empty);
            return;
        }
        
        // Otherwise, go to previous section if available
        if (CurrentSectionIndex > 0)
        {
            CurrentSectionIndex--;
            CurrentSection = Examination.ExaminationSections[CurrentSectionIndex];
            
            if (CurrentSection.Questions != null && CurrentSection.Questions.Length > 0)
            {
                CurrentQuestionIndex = CurrentSection.Questions.Length - 1;
                CurrentQuestion = CurrentSection.Questions[CurrentQuestionIndex];
                QuestionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public void NavigateNext()
    {
        if (Examination == null || CurrentSection == null) return;
        
        // Save current answer before navigating
        SaveCurrentAnswer();
        
        // Check if we can navigate to next question in current section
        if (CurrentSection.Questions != null && CurrentQuestionIndex < CurrentSection.Questions.Length - 1)
        {
            CurrentQuestionIndex++;
            CurrentQuestion = CurrentSection.Questions[CurrentQuestionIndex];
            QuestionChanged?.Invoke(this, EventArgs.Empty);
            return;
        }
        
        // Otherwise, go to next section if available
        if (CurrentSectionIndex < Examination.ExaminationSections.Length - 1)
        {
            CurrentSectionIndex++;
            CurrentSection = Examination.ExaminationSections[CurrentSectionIndex];
            
            if (CurrentSection.Questions != null && CurrentSection.Questions.Length > 0)
            {
                CurrentQuestionIndex = 0;
                CurrentQuestion = CurrentSection.Questions[0];
                QuestionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    

    public async Task SaveProgressSilently()
    {
        if (Examination == null) return;
        
        try
        {
            // Save current answer (handled by the view)
            SaveCurrentAnswer();
            
            // Create a deep copy to avoid issues with Options
            var examinationJson = ExaminationSerializer.SerializeToJson(Examination);
            var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson);
            
            if (examinationCopy != null)
            {
                // Update the examination in app data
                _configService.AppData.CurrentExamination = examinationCopy;
                _configService.AppData.IsInExamination = true;
                
                // Set or update timer
                if (!_configService.AppData.ExaminationTimer.HasValue)
                {
                    _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                
                await _configService.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Silently log the error
            Console.WriteLine($"Error saving progress silently: {ex.Message}");
        }
    }
    
    public async Task SaveProgress()
    {
        if (Examination == null) return;
        
        IsSaving = true;
        StatusMessage = _localizationService["exam.save.progress"];
        ShowStatusMessage = true;
        
        try
        {
            // Save current answer (handled by the view)
            SaveCurrentAnswer();
            
            // Create a deep copy to avoid issues with Options
            var examinationJson = ExaminationSerializer.SerializeToJson(Examination);
            var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson);
            
            if (examinationCopy != null)
            {
                // Update the examination in app data
                _configService.AppData.CurrentExamination = examinationCopy;
                _configService.AppData.IsInExamination = true;
                
                // Set or update timer
                if (!_configService.AppData.ExaminationTimer.HasValue)
                {
                    _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                
                await _configService.SaveChangesAsync();
                
                StatusMessage = _localizationService["exam.save.success"];
                await Task.Delay(2000); // Show success message for 2 seconds
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["exam.save.error"]}: {ex.Message}";
            await Task.Delay(5000); // Show error message for 5 seconds
        }
        finally
        {
            IsSaving = false;
            ShowStatusMessage = false;
        }
    }
    
    public async Task SubmitExamination()
    {
        if (Examination == null) return;
    
        // Save final answers
        SaveCurrentAnswer();
    
        // Calculate scores
        var scoreRecord = new ScoreRecord
        {
            ExamId = Examination.ExaminationMetadata.ExamId ?? string.Empty,
            ExamTitle = Examination.ExaminationMetadata.Title,
            UserName = _configService.AppConfig.UserName
        };
    
        // Calculate scores
        scoreRecord.CalculateScores(Examination);
    
        // Create a deep copy to avoid issues with Options
        var examinationJson = ExaminationSerializer.SerializeToJson(Examination);
        var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson!);
    
        if (examinationCopy != null)
        {
            // Save examination with final scores
            _configService.AppData.CurrentExamination = examinationCopy;
            _configService.AppData.IsInExamination = false;
            await _configService.SaveChangesAsync();
        }

        IsWindowVisible = false;
        var resultWindow = new ExaminationResultWindow(Examination, scoreRecord);
        resultWindow.Closed += (sender, args) =>
        {
            WindowCloseRequested?.Invoke(this, EventArgs.Empty);
        };
        resultWindow.Show();
    }

    
    public void BackToMain()
    {
        // Save progress before exiting
        SaveCurrentAnswer();
        SaveProgressSilently();
        
        // TODO: Navigate back to main window
    }
    
    public string GetLocalizedQuestionType(QuestionTypes type)
    {
        string key = type switch
        {
            QuestionTypes.SingleChoice => "exam.question.type.single",
            QuestionTypes.MultipleChoice => "exam.question.type.multiple",
            QuestionTypes.Judgment => "exam.question.type.judgment",
            QuestionTypes.FillInTheBlank => "exam.question.type.fill",
            QuestionTypes.Math => "exam.question.type.math",
            QuestionTypes.Essay => "exam.question.type.essay",
            QuestionTypes.ShortAnswer => "exam.question.type.short",
            QuestionTypes.Calculation => "exam.question.type.calculation",
            QuestionTypes.Complex => "exam.question.type.complex",
            QuestionTypes.Other => "exam.question.type.other",
            _ => "exam.question.type.other"
        };
        
        return _localizationService[key];
    }
    
    public string GetFormattedString(string key, params object[] args)
    {
        string template = _localizationService[key];
        return string.Format(template, args);
    }
}
