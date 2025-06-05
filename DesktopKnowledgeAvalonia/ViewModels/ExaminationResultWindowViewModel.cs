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
    private string _resultStatusText = string.Empty;

    [ObservableProperty]
    private string _subStatusText = string.Empty;

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
    private ScoreRecord _scoreRecord = new();

    [ObservableProperty]
    private Examination _examination = new();

    [ObservableProperty] 
    private ObservableCollection<SectionScoreViewModel> _sectionScores = new();

    [ObservableProperty] 
    private bool _isQuestionsPanelExpanded = true;

    [ObservableProperty] 
    private bool _isWindowVisible = true;
    
    [ObservableProperty]
    private bool _hasPerformedInitialAiScoring = false;

    // Flag to control main window visibility
    public bool ShowMainWindow { get; set; } = false;

    // Event to request saving the examination
    public event EventHandler<SaveExaminationEventArgs>? SaveExaminationRequested;
    public event EventHandler? ExitRequested;

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
    
        // Initialize AI questions evaluation status
        InitializeAiQuestionsEvaluationStatus();
    
        // Recalculate scores after initialization
        ScoreRecord.CalculateScores(Examination);
    
        ObtainedScore = ScoreRecord.ObtainedScore;
        TotalScore = ScoreRecord.TotalScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
    
        // Determine pass/fail status (60% is passing)
        IsPassed = ScorePercentage >= 60;
        UpdateResultStatusText();
    
        // Check if any questions need AI scoring
        IsAiScoringNeeded = CheckIfAiScoringNeeded();
    
        // Initialize question scores collection
        InitializeQuestionScores();
    }

    // Method to initialize AI questions evaluation status
    private void InitializeAiQuestionsEvaluationStatus()
    {
        if (Examination?.ExaminationSections == null)
            return;
        
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null)
                continue;
            
            foreach (var question in section.Questions)
            {
                // 只看 IsAiJudge 标记，不考虑题目类型
                if (question.IsAiJudge)
                {
                    // Check if this AI question has been evaluated
                    bool hasAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0;
                    
                    if (hasAnswer)
                    {
                        // If there's AI feedback, consider it evaluated
                        if (!string.IsNullOrEmpty(question.AiFeedback))
                        {
                            question.IsAiEvaluated = true;
                            // ObtainedScore should already be set by AI
                        }
                        else
                        {
                            // Not yet evaluated by AI
                            question.IsAiEvaluated = false;
                            question.ObtainedScore = 0.0; // Set obtained score to 0, but keep original Score (max score)
                        }
                    }
                    else
                    {
                        // No answer provided, consider evaluated with 0 score
                        question.IsAiEvaluated = true;
                        question.ObtainedScore = 0.0;
                    }
                }
                else
                {
                    // Non-AI questions are always considered evaluated
                    question.IsAiEvaluated = true;
                }
            }
        }
    }

    private void UpdateResultStatusText()
    {
        if (IsAiScoringInProgress)
        {
            ResultStatusText = _localizationService["exam.result.scoring.in.progress"];
        }
        else
        {
            ResultStatusText = IsPassed 
                ? _localizationService["exam.result.passed"] 
                : _localizationService["exam.result.failed"];
        }
    }

    private bool CheckIfAiScoringNeeded()
    {
        if (Examination?.ExaminationSections == null) 
            return false;
    
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) 
                continue;
        
            foreach (var question in section.Questions)
            {
                // A question needs AI scoring if:
                // 1. It's marked for AI judging
                // 2. User provided an answer
                // 3. It hasn't been evaluated yet
                if (question.IsAiJudge && 
                    question.UserAnswer != null && 
                    question.UserAnswer.Length > 0 && 
                    !question.IsAiEvaluated)
                {
                    return true;
                }
            }
        }
    
        return false;
    }

    [RelayCommand]
    private void ExportScore()
    {
        // TODO: Implement export functionality
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void ToggleQuestionsPanel()
    {
        IsQuestionsPanelExpanded = !IsQuestionsPanelExpanded;
    }

    private void InitializeQuestionScores()
    {
        var newSections = new ObservableCollection<SectionScoreViewModel>();
    
        if (Examination?.ExaminationSections == null || ScoreRecord == null) 
        {
            SectionScores = newSections;
            return;
        }
    
        var allScores = ScoreRecord.GetAllQuestionScores();
    
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
            
                if (allScores.TryGetValue(questionId, out var score))
                {
                    string correctAnswer = "";
                    if (question.Answer != null && question.Answer.Length > 0)
                    {
                        correctAnswer = string.Join(", ", question.Answer);
                    }

                    sectionVM.Questions.Add(new QuestionScoreViewModel
                    {
                        QuestionNumber = questionNumber++,
                        SectionTitle = section.Title,
                        QuestionId = questionId,
                        QuestionType = question.Type,
                        QuestionStem = question.Stem,
                        MaxScore = question.Score, // Use question's original score as max score
                        ObtainedScore = score.ObtainedScore,
                        IsCorrect = score.IsCorrect,
                        IsAiJudged = question.IsAiJudge,
                        IsEvaluated = question.IsAiEvaluated, // Use question's evaluation status
                        UserAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0 
                            ? string.Join(", ", question.UserAnswer) 
                            : _localizationService["exam.result.no.answer"],
                        CorrectAnswer = correctAnswer,
                        AiFeedback = question.AiFeedback ?? string.Empty // Use question's AI feedback
                    });
                }
            }
        
            if (sectionVM.Questions.Count > 0)
            {
                newSections.Add(sectionVM);
            }
        }
    
        SectionScores = newSections;
    }

    [RelayCommand]
    private async Task StartAiScoringAsync()
    {
        if (!IsAiScoringNeeded || IsAiScoringInProgress) 
            return;
    
        IsAiScoringInProgress = true;
        CanDownloadExam = false;
        CanExit = false;
        CanExportScore = false;
        AiScoringProgress = 0;
        UpdateResultStatusText();
    
        try
        {
            await PerformAiScoring();
            IsAiScoringNeeded = false; // No longer needed after completion
            HasPerformedInitialAiScoring = true; // 设置已完成初始AI评分
        }
        finally
        {
            IsAiScoringInProgress = false;
            CanDownloadExam = true;
            CanExit = true;
            // CanExportScore = true;
            CurrentScoringQuestion = string.Empty;
            CurrentScoringQuestionProgress = string.Empty;
        
            UpdateResultStatusText();
        }
    }

    private async Task PerformAiScoring()
    {
        if (Examination?.ExaminationSections == null) 
            return;
    
        var aiClient = AiTools.CreateOpenAiClient(_configService.SystemConfig);
    
        // Count questions needing AI scoring
        var questionsToScore = new List<(ExaminationSection section, Question question)>();
    
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) 
                continue;
        
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && 
                    question.UserAnswer != null && 
                    question.UserAnswer.Length > 0 &&
                    !question.IsAiEvaluated)
                {
                    questionsToScore.Add((section, question));
                }
            }
        }
    
        if (questionsToScore.Count == 0) 
            return;
    
        // Process each question
        for (int i = 0; i < questionsToScore.Count; i++)
        {
            var (section, question) = questionsToScore[i];
        
            // Update progress display
            CurrentScoringQuestionProgress = $"{i + 1} / {questionsToScore.Count}";
        
            string cleanStem = question.Stem
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ");
            
            CurrentScoringQuestion = $"{section.Title}: {cleanStem.Substring(0, Math.Min(50, cleanStem.Length))}...";
        
            try
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
                
                    // Update question with AI results
                    question.ObtainedScore = result.Score;
                    question.AiFeedback = result.Feedback;
                    question.IsAiEvaluated = true; // Mark as evaluated
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scoring question {question.QuestionId}: {ex.Message}");
                // Mark as evaluated even if scoring failed
                question.IsAiEvaluated = true;
                question.ObtainedScore = 0.0;
                question.AiFeedback = "Error occurred during AI evaluation.";
            }
        
            // Update progress
            AiScoringProgress = (double)(i + 1) / questionsToScore.Count * 100;
        
            // Recalculate scores and regenerate UI after each question
            ScoreRecord.CalculateScores(Examination);
            ObtainedScore = ScoreRecord.ObtainedScore;
            ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
            IsPassed = ScorePercentage >= 60;
            
            // Trigger complete UI regeneration
            InitializeQuestionScores();
        }
    }

    [RelayCommand]
    private void SaveExamination()
    {
        if (Examination == null || !CanDownloadExam) 
            return;
    
        SaveExaminationRequested?.Invoke(this, new SaveExaminationEventArgs(Examination));
    }

    [RelayCommand]
    private void Exit()
    {
        if (!CanExit) 
            return;
    
        // Clear current examination data
        _configService.AppData.CurrentExamination = null;
        _configService.AppData.IsInExamination = false;
        _configService.AppData.IsTheExaminationStarted = false;
        _configService.AppData.ExaminationTimer = null;
    
        ShowMainWindow = true;
        _configService.SaveChangesAsync();
    
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task RescoreQuestionAsync(string questionId)
    {
        if (Examination == null || string.IsNullOrEmpty(questionId) || 
            IsAiScoringInProgress || !HasPerformedInitialAiScoring)
            return;
        
        // Find the question
        Question? questionToScore = null;
        string sectionTitle = string.Empty;
    
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) 
                continue;
        
            foreach (var question in section.Questions)
            {
                if (question.QuestionId == questionId)
                {
                    questionToScore = question;
                    sectionTitle = section.Title;
                    break;
                }
            }
        
            if (questionToScore != null) 
                break;
        }
    
        // 验证题目可以被重新评分：必须是AI评分题且有答案
        if (questionToScore == null || !questionToScore.IsAiJudge || 
            questionToScore.UserAnswer == null || questionToScore.UserAnswer.Length == 0)
            return;
        
        // Start rescoring
        IsAiScoringInProgress = true;
        CanDownloadExam = false;
        CanExit = false;
        CanExportScore = false;
        UpdateResultStatusText();
    
        string cleanStem = questionToScore.Stem
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ");
        
        CurrentScoringQuestionProgress = $"1 / 1 ({_localizationService["exam.result.rescore"]})";
        CurrentScoringQuestion = $"{sectionTitle}: {cleanStem.Substring(0, Math.Min(50, cleanStem.Length))}...";
    
        try
        {
            var aiClient = AiTools.CreateOpenAiClient(_configService.SystemConfig);
        
            string prompt = PromptTemplateManager.GenerateGradingPrompt(
                questionToScore, 
                _configService.AppConfig.PromptGradingTemplate,
                true,
                _localizationService.CurrentLanguage);
        
            string? response = await AiTools.SendChatMessageAsync(
                aiClient,
                _configService.SystemConfig,
                prompt);
        
            if (!string.IsNullOrEmpty(response))
            {
                var result = PromptTemplateManager.ParseAIResponse(response);
                
                // Update question with new AI results
                questionToScore.ObtainedScore = result.Score;
                questionToScore.AiFeedback = result.Feedback;
                questionToScore.IsAiEvaluated = true;
            
                // Recalculate scores and regenerate UI
                ScoreRecord.CalculateScores(Examination);
                ObtainedScore = ScoreRecord.ObtainedScore;
                ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
                IsPassed = ScorePercentage >= 60;
            
                // Trigger complete UI regeneration
                InitializeQuestionScores();
            }
        
            AiScoringProgress = 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rescoring question {questionId}: {ex.Message}");
            questionToScore.AiFeedback = "Error occurred during AI evaluation.";
        }
        finally
        {
            IsAiScoringInProgress = false;
            CanDownloadExam = true;
            CanExit = true;
            // CanExportScore = true;
            CurrentScoringQuestion = string.Empty;
            CurrentScoringQuestionProgress = string.Empty;
        
            UpdateResultStatusText();
        }
    }

    [RelayCommand]
    private async Task RescoreAllAsync()
    {
        if (Examination == null || IsAiScoringInProgress || !HasPerformedInitialAiScoring) 
            return;
    
        // Reset all AI questions to unevaluated state
        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) 
                continue;
        
            foreach (var question in section.Questions)
            {
                if (question.IsAiJudge && 
                    question.UserAnswer != null && 
                    question.UserAnswer.Length > 0)
                {
                    question.ObtainedScore = 0.0;
                    question.AiFeedback = null;
                    question.IsAiEvaluated = false;
                }
            }
        }
    
        // Recalculate scores
        ScoreRecord.CalculateScores(Examination);
        ObtainedScore = ScoreRecord.ObtainedScore;
        ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
        IsPassed = ScorePercentage >= 60;
    
        // Set AI scoring as needed
        IsAiScoringNeeded = true;
    
        // Refresh question scores to show updated states
        InitializeQuestionScores();
    
        // Start AI scoring
        await StartAiScoringAsync();
    }
}

// Event args and helper classes remain the same
public class SaveExaminationEventArgs : EventArgs
{
    public Examination Examination { get; }

    public SaveExaminationEventArgs(Examination examination)
    {
        Examination = examination;
    }
}

public class SectionScoreViewModel : ViewModelBase
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public ObservableCollection<QuestionScoreViewModel> Questions { get; set; } = new();
}

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
    public bool IsEvaluated { get; set; } = true;
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string AiFeedback { get; set; } = string.Empty;
}
