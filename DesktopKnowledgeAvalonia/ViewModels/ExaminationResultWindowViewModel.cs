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
    private string _currentScoringQuestion = string.Empty;
    
    [ObservableProperty]
    private string _currentScoringQuestionProgress = string.Empty;
    
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
    private ObservableCollection<SectionScoreViewModel> _sectionScores = new();
    
    [ObservableProperty] 
    private bool _isQuestionsPanelExpanded = true;
    
    [ObservableProperty] 
    private bool _isWindowVisible = true;
    
    // Flag to control main window visibility
    public bool ShowMainWindow { get; set; } = false;
    
    // Event to request saving the examination
    public event EventHandler<SaveExaminationEventArgs> SaveExaminationRequested;
    public event EventHandler ExitRequested;
    
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
        
        // For unevaluated AI questions, set their score to 0
        SetUnevaluatedAiQuestionsToZero();
        
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
    
    // Method to set unevaluated AI questions to 0
    private void SetUnevaluatedAiQuestionsToZero()
    {
        if (Examination == null || ScoreRecord == null)
            return;
            
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null)
                continue;
                
            foreach (var question in section.Questions)
            {
                // For AI judged questions that haven't been evaluated, set score to 0
                if (question.IsAiJudge && question.Score == null)
                {
                    question.Score = 0;
                }
            }
        }
        
        // Recalculate scores
        ScoreRecord.CalculateScores(Examination);
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
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0 && question.Score == null)
                {
                    // If user provided an answer for an AI-judged question that hasn't been evaluated
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
    
    [RelayCommand]
    private void ToggleQuestionsPanel()
    {
        IsQuestionsPanelExpanded = !IsQuestionsPanelExpanded;
    }
    
    private void InitializeQuestionScores()
    {
        // Create a new collection to trigger property changed notification
        var newSections = new ObservableCollection<SectionScoreViewModel>();
        
        if (Examination == null || ScoreRecord == null) 
        {
            SectionScores = newSections;
            return;
        }
        
        // Get all question scores from the score record
        var allScores = ScoreRecord.GetAllQuestionScores();
        
        // Create view models for each section and question
        int questionNumber = 1;
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null || section.Questions.Length == 0) 
                continue;
            
            var sectionVM = new SectionScoreViewModel
            {
                SectionId = section.SectionId ?? string.Empty,
                SectionTitle = section.Title,
                Questions = new ObservableCollection<QuestionScoreViewModel>()
            };
            
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

                    // Determine if the question has been evaluated
                    bool isEvaluated = true;
                    if (question.IsAiJudge && question.Score == null)
                    {
                        isEvaluated = false;
                        // For unevaluated AI questions, show 0.0 as obtained score
                        score.ObtainedScore = 0.0;
                        score.IsCorrect = false;
                    }

                    sectionVM.Questions.Add(new QuestionScoreViewModel
                    {
                        QuestionNumber = questionNumber++,
                        SectionTitle = section.Title,
                        QuestionId = questionId,
                        QuestionType = question.Type,
                        QuestionStem = question.Stem,
                        MaxScore = score.MaxScore,
                        ObtainedScore = score.ObtainedScore,
                        IsCorrect = score.IsCorrect,
                        IsAiJudged = question.IsAiJudge,
                        IsEvaluated = isEvaluated,
                        UserAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0 
                            ? string.Join(", ", question.UserAnswer) 
                            : _localizationService["exam.result.no.answer"],
                        CorrectAnswer = correctAnswer,
                        AiFeedback = string.Empty // Will be populated after AI scoring
                    });
                }
            }
            
            // Only add sections that have questions
            if (sectionVM.Questions.Count > 0)
            {
                newSections.Add(sectionVM);
            }
        }
        
        // Set the new collection to trigger property changed notification
        SectionScores = newSections;
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
            CurrentScoringQuestion = string.Empty;
            CurrentScoringQuestionProgress = string.Empty;
            
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
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0 && question.Score == null)
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
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0 && question.Score == null)
                {
                    processedQuestions++;
                    
                    // Update the currently scoring question text and progress
                    string cleanStem = question.Stem
                        .Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("\r", " ");
                        
                    CurrentScoringQuestionProgress = $"{processedQuestions} ({_localizationService["exam.result.ai.scored"]}) / {totalAiQuestions} ({_localizationService["exam.result.ai.scored"]})";
                    CurrentScoringQuestion = $"{section.Title}: {cleanStem.Substring(0, Math.Min(50, cleanStem.Length))}...";
                    
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
                        UpdateQuestionScore(question.QuestionId, result.Score, result.IsCorrect, true, result.Feedback);
                        
                        // Recalculate scores after each question to show progress
                        ScoreRecord.CalculateScores(Examination);
                        
                        // Update UI with new scores
                        ObtainedScore = ScoreRecord.ObtainedScore;
                        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
                        IsPassed = ScorePercentage >= 60;
                    }
                    
                    // Update progress
                    AiScoringProgress = (double)processedQuestions / totalAiQuestions * 100;
                }
            }
        }
        
        // Final recalculation (should be redundant but ensures consistency)
        ScoreRecord.CalculateScores(Examination);
        
        // Update UI with new scores
        ObtainedScore = ScoreRecord.ObtainedScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
        IsPassed = ScorePercentage >= 60;
        
        // Refresh question scores
        InitializeQuestionScores();
    }
    
    private void UpdateQuestionScore(string? questionId, double score, bool isCorrect, bool isEvaluated = true, string aiFeedback = "")
    {
        if (string.IsNullOrEmpty(questionId)) return;
        
        foreach (var section in SectionScores)
        {
            var questionVm = section.Questions.FirstOrDefault(q => q.QuestionId == questionId);
            if (questionVm != null)
            {
                questionVm.ObtainedScore = score;
                questionVm.IsCorrect = isCorrect;
                questionVm.IsEvaluated = isEvaluated;
                questionVm.AiFeedback = aiFeedback;
                return;
            }
        }
    }
    
    [RelayCommand]
    private void SaveExamination()
    {
        if (Examination == null || !CanDownloadExam) return;
        
        // Raise event to be handled by the code-behind
        SaveExaminationRequested?.Invoke(this, new SaveExaminationEventArgs(Examination));
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
        
        // Set flag to show main window after closing
        ShowMainWindow = true;
        
        // Save changes
        _configService.SaveChangesAsync();
        
        // Notify view to close window
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private async Task RescoreQuestionAsync(string questionId)
    {
        if (Examination == null || string.IsNullOrEmpty(questionId) || IsAiScoringInProgress)
            return;
            
        // Find the question
        Question? questionToScore = null;
        string sectionTitle = string.Empty;
        
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                if (question.QuestionId == questionId)
                {
                    questionToScore = question;
                    sectionTitle = section.Title;
                    break;
                }
            }
            
            if (questionToScore != null) break;
        }
        
        if (questionToScore == null || !questionToScore.IsAiJudge || 
            questionToScore.UserAnswer == null || questionToScore.UserAnswer.Length == 0)
            return;
            
        // Start scoring just this question
        IsAiScoringInProgress = true;
        CanDownloadExam = false;
        CanExit = false;
        ResultStatusText = _localizationService["exam.result.scoring.in.progress"];
        
        // Format the question stem to handle line breaks properly
        string cleanStem = questionToScore.Stem
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");
            
        CurrentScoringQuestionProgress = $"1 ({_localizationService["exam.result.rescore"]}) / 1 ({_localizationService["exam.result.rescore"]})";
        CurrentScoringQuestion = $"{sectionTitle}: {cleanStem.Substring(0, Math.Min(50, cleanStem.Length))}...";
        
        try
        {
            // Get AI client
            var aiClient = AiTools.CreateOpenAiClient(_configService.SystemConfig);
            
            // Generate prompt for AI scoring
            string prompt = PromptTemplateManager.GenerateGradingPrompt(
                questionToScore, 
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
                questionToScore.Score = result.Score;
                
                // Update UI for this question
                UpdateQuestionScore(questionId, result.Score, result.IsCorrect, true, result.Feedback);
                
                // Recalculate scores
                ScoreRecord.CalculateScores(Examination);
                
                // Update UI with new scores
                ObtainedScore = ScoreRecord.ObtainedScore;
                ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
                IsPassed = ScorePercentage >= 60;
                
                // Refresh question scores
                InitializeQuestionScores();
            }
            
            AiScoringProgress = 100;
        }
        finally
        {
            IsAiScoringInProgress = false;
            CanDownloadExam = true;
            CanExit = true;
            CurrentScoringQuestion = string.Empty;
            CurrentScoringQuestionProgress = string.Empty;
            
            // Update status text
            UpdateResultStatusText();
        }
    }

    [RelayCommand]
    private async Task RescoreAllAsync()
    {
        if (Examination == null || IsAiScoringInProgress) return;
        
        // Reset all AI questions to unscored state
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) continue;
            
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && question.UserAnswer != null && question.UserAnswer.Length > 0)
                {
                    question.Score = 0.0d; // Reset to unscored
                }
            }
        }
        
        // Recalculate scores with reset values
        SetUnevaluatedAiQuestionsToZero();
        
        // Update UI
        ObtainedScore = ScoreRecord.ObtainedScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
        IsPassed = ScorePercentage >= 60;
        
        // Check if scoring is needed again
        IsAiScoringNeeded = CheckIfAiScoringNeeded();
        
        // Refresh question scores
        InitializeQuestionScores();
        
        // Start AI scoring if needed
        if (IsAiScoringNeeded)
        {
            await StartAiScoringAsync();
        }
    }
}

// Event args for save examination request
public class SaveExaminationEventArgs : EventArgs
{
    public Examination Examination { get; }
    
    public SaveExaminationEventArgs(Examination examination)
    {
        Examination = examination;
    }
}

// Helper class for section organization
public class SectionScoreViewModel : ViewModelBase
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public ObservableCollection<QuestionScoreViewModel> Questions { get; set; } = new();
}

// Helper class for question score display
public class QuestionScoreViewModel : ViewModelBase
{
    public int QuestionNumber { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public QuestionTypes QuestionType { get; set; }
    public string QuestionStem { get; set; } = string.Empty;
    public double MaxScore { get; set; }
    public double ObtainedScore { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsAiJudged { get; set; }
    public bool IsEvaluated { get; set; } = true; // Added property to track if AI has evaluated this question
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string AiFeedback { get; set; } = string.Empty; // Add AI feedback property
}
