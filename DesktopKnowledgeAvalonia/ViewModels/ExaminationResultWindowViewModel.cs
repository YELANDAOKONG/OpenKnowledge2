using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.Tools;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;
using OpenAI;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class ExaminationResultWindowViewModel : ViewModelBase
{
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    
    [ObservableProperty]
    private double _obtainedScore;
    
    [ObservableProperty]
    private double _totalScore;
    
    [ObservableProperty]
    private double _scorePercentage;
    
    [ObservableProperty]
    private bool _isPassed;
    
    [ObservableProperty]
    private string _resultStatusText;
    
    [ObservableProperty]
    private string _subStatusText;
    
    [ObservableProperty]
    private bool _isAiScoringNeeded;
    
    [ObservableProperty]
    private bool _isAiScoringInProgress;
    
    [ObservableProperty]
    private double _aiScoringProgress;
    
    [ObservableProperty]
    private bool _canExportScore = false;
    
    [ObservableProperty]
    private bool _canDownloadExam = true;
    
    [ObservableProperty]
    private bool _canExit = true;
    
    [ObservableProperty]
    private ScoreRecord _scoreRecord;
    
    [ObservableProperty]
    private Examination _examination;
    
    [ObservableProperty] 
    private ObservableCollection<QuestionScoreViewModel> _questionScores = new();

    [ObservableProperty] 
    private bool _isWindowsVisible = true;
    
    public ExaminationResultWindowViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        
        // Default values
        SubStatusText = _localizationService["exam.result.thank.you"];
    }
    
    public async Task InitializeAsync(Examination examination, ScoreRecord scoreRecord)
    {
        Examination = examination;
        ScoreRecord = scoreRecord;
        
        ObtainedScore = scoreRecord.ObtainedScore;
        TotalScore = scoreRecord.TotalScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
        
        // Determine pass/fail status (60% is passing)
        IsPassed = ScorePercentage >= 60;
        UpdateResultStatusText();
        
        // Check if any questions need AI scoring
        IsAiScoringNeeded = CheckIfAiScoringNeeded();
        
        // Initialize question scores collection
        InitializeQuestionScores();
    }
    
    private void UpdateResultStatusText()
    {
        ResultStatusText = IsPassed 
            ? _localizationService["exam.result.passed"] 
            : _localizationService["exam.result.failed"];
            
        if (IsAiScoringInProgress)
        {
            ResultStatusText = _localizationService["exam.result.scoring.in.progress"];
        }
    }
    
    private bool CheckIfAiScoringNeeded()
    {
        if (Examination == null) return false;
        
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    [RelayCommand]
    private void ExportSocre()
    {
        throw new NotImplementedException();
    }
    
    private void InitializeQuestionScores()
    {
        // Create a new collection to trigger property changed notification
        var newScores = new ObservableCollection<QuestionScoreViewModel>();
        
        if (Examination == null || ScoreRecord == null) 
        {
            QuestionScores = newScores;
            return;
        }
        
        // Get all question scores from the score record
        var allScores = ScoreRecord.GetAllQuestionScores();
        
        // Create view models for each section and question
        int questionNumber = 1;
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                string questionId = question.QuestionId ?? string.Empty;
                
                // Find the score for this question
                if (allScores.TryGetValue(questionId, out var score))
                {
                    string correctAnswer = "";
                    if (question.Answer != null && question.Answer.Length > 0)
                    {
                        correctAnswer = string.Join(", ", question.Answer);
                    }

                    newScores.Add(new QuestionScoreViewModel
                    {
                        QuestionNumber = questionNumber++,
                        SectionTitle = section.Title,
                        QuestionId = questionId,
                        QuestionType = question.Type,
                        QuestionStem = question.Stem,
                        MaxScore = score.MaxScore,
                        ObtainedScore = score.ObtainedScore,
                        IsCorrect = score.IsCorrect,
                        UserAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0 
                            ? string.Join(", ", question.UserAnswer) 
                            : _localizationService["exam.result.no.answer"],
                        CorrectAnswer = correctAnswer
                    });
                }
            }
        }
        
        // Set the new collection to trigger property changed notification
        QuestionScores = newScores;
    }
    
    [RelayCommand]
    private async Task StartAiScoringAsync()
    {
        if (!IsAiScoringNeeded || IsAiScoringInProgress) return;
        
        IsAiScoringInProgress = true;
        CanDownloadExam = false;
        CanExit = false;
        AiScoringProgress = 0;
        ResultStatusText = _localizationService["exam.result.scoring.in.progress"];
        
        try
        {
            await PerformAiScoring();
        }
        finally
        {
            IsAiScoringInProgress = false;
            CanDownloadExam = true;
            CanExit = true;
            IsAiScoringNeeded = false; // No longer needed after completion
            
            // Update status text
            UpdateResultStatusText();
        }
    }
    
    private async Task PerformAiScoring()
    {
        if (Examination == null) return;
        
        // Get AI client
        var aiClient = AiTools.CreateOpenAiClient(_configService.SystemConfig);
        
        // Count questions needing AI scoring
        int totalAiQuestions = 0;
        int processedQuestions = 0;
        
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0)
                {
                    totalAiQuestions++;
                }
            }
        }
        
        if (totalAiQuestions == 0) return;
        
        // Process each question needing AI scoring
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0)
                {
                    // Generate prompt for AI scoring
                    string prompt = PromptTemplateManager.GenerateGradingPrompt(
                        question, 
                        _configService.AppConfig.PromptGradingTemplate,
                        true,
                        _localizationService.CurrentLanguage);
                    
                    // Send to AI for scoring
                    string? response = await AiTools.SendChatMessageAsync(
                        aiClient,
                        _configService.SystemConfig,
                        prompt);
                    
                    if (!string.IsNullOrEmpty(response))
                    {
                        // Parse AI response
                        var result = PromptTemplateManager.ParseAIResponse(response);
                        
                        // Update question score
                        question.Score = result.Score;
                        
                        // Update UI for this question
                        UpdateQuestionScore(question.QuestionId, result.Score, result.IsCorrect);
                    }
                    
                    // Update progress
                    processedQuestions++;
                    AiScoringProgress = (double)processedQuestions / totalAiQuestions * 100;
                }
            }
        }
        
        // Recalculate scores
        ScoreRecord.CalculateScores(Examination);
        
        // Update UI with new scores
        ObtainedScore = ScoreRecord.ObtainedScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
        IsPassed = ScorePercentage >= 60;
        
        // Refresh question scores
        InitializeQuestionScores();
    }
    
    private void UpdateQuestionScore(string? questionId, double score, bool isCorrect)
    {
        if (string.IsNullOrEmpty(questionId)) return;
        
        var questionVm = QuestionScores.FirstOrDefault(q => q.QuestionId == questionId);
        if (questionVm != null)
        {
            questionVm.ObtainedScore = score;
            questionVm.IsCorrect = isCorrect;
        }
    }
    
    [RelayCommand]
    private async Task SaveExaminationAsync()
    {
        if (Examination == null || !CanDownloadExam) return;
        
        try
        {
            // Create file picker options
            var options = new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = _localizationService["exam.dialog.save.title"],
                SuggestedFileName = $"exam_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                DefaultExtension = "json",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("JSON")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" }
                    },
                    new Avalonia.Platform.Storage.FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" },
                        MimeTypes = new[] { "*/*" }
                    }
                }
            };
            
            // Get active window from app lifecycle
            var window = Avalonia.Application.Current?.ApplicationLifetime is 
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null;
            
            if (window == null) return;
            
            // Show save dialog
            var result = await window.StorageProvider.SaveFilePickerAsync(options);
            
            if (result != null)
            {
                // Get file path
                var filePath = result.Path.LocalPath;
                
                // Save examination with user answers
                ExaminationSerializer.SerializeToFile(
                    Examination,
                    filePath,
                    includeUserAnswers: true);
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error saving examination: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void Exit()
    {
        if (!CanExit) return;
        
        // Clear current examination data
        _configService.AppData.CurrentExamination = null;
        _configService.AppData.IsInExamination = false;
        _configService.AppData.IsTheExaminationStarted = false;
        _configService.AppData.ExaminationTimer = null;
        _configService.SaveChangesAsync();
        
        // Close window (handled by view)
        IsWindowsVisible = false;
    }
}

// Helper class for question score display
public class QuestionScoreViewModel
{
    public int QuestionNumber { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public QuestionTypes QuestionType { get; set; }
    public string QuestionStem { get; set; } = string.Empty;
    public double MaxScore { get; set; }
    public double ObtainedScore { get; set; }
    public bool IsCorrect { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}
