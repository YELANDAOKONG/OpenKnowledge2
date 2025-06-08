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
                InitializeQuestionAiStatus(question);
                
                // 处理子问题
                if (question.SubQuestions != null && question.SubQuestions.Count > 0)
                {
                    foreach (var subQuestion in question.SubQuestions)
                    {
                        InitializeQuestionAiStatus(subQuestion);
                    }
                }
            }
        }
    }

    // 抽取方法处理单个问题的AI状态初始化
    private void InitializeQuestionAiStatus(Question question)
    {
        if (question.IsAiJudge)
        {
            // 检查这个AI问题是否已被评估
            bool hasAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0;
            
            if (hasAnswer)
            {
                // 如果有AI反馈，认为已评估
                if (!string.IsNullOrEmpty(question.AiFeedback))
                {
                    question.IsAiEvaluated = true;
                    // ObtainedScore应该已由AI设置
                }
                else
                {
                    // 尚未由AI评估
                    question.IsAiEvaluated = false;
                    question.ObtainedScore = 0.0; // 设置获得分数为0，但保留原始分数（最高分）
                }
            }
            else
            {
                // 未提供答案，认为已评估，分数为0
                question.IsAiEvaluated = true;
                question.ObtainedScore = 0.0;
            }
        }
        else
        {
            // 非AI问题始终视为已评估
            question.IsAiEvaluated = true;
        }
    }

    

    // 判断单个问题是否需要AI评分
    private bool IsQuestionNeedsAiScoring(Question question)
    {
        return question.IsAiJudge && 
               question.UserAnswer != null && 
               question.UserAnswer.Length > 0 && 
               !question.IsAiEvaluated;
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
                // 检查主问题是否需要AI评分
                if (IsQuestionNeedsAiScoring(question))
                    return true;
                
                // 检查子问题是否需要AI评分
                if (question.SubQuestions != null)
                {
                    foreach (var subQuestion in question.SubQuestions)
                    {
                        if (IsQuestionNeedsAiScoring(subQuestion))
                            return true;
                    }
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
                    // 创建主问题的ViewModel
                    var questionViewModel = CreateQuestionScoreViewModel(
                        questionNumber++, 
                        section.Title, 
                        question, 
                        score);
                    
                    // 处理复合题的子问题
                    if (question.Type == QuestionTypes.Complex && 
                        question.SubQuestions != null && 
                        question.SubQuestions.Count > 0)
                    {
                        // 递归处理子问题
                        ProcessSubQuestions(questionViewModel, question, 0);
                    }
                    
                    sectionVM.Questions.Add(questionViewModel);
                }
            }
            
            if (sectionVM.Questions.Count > 0)
            {
                newSections.Add(sectionVM);
            }
        }
        
        SectionScores = newSections;
    }

    // 创建单个问题的ViewModel
    private QuestionScoreViewModel CreateQuestionScoreViewModel(
        int number, 
        string sectionTitle, 
        Question question, 
        QuestionScore score)
    {
        string correctAnswer = "";
        if (question.Answer != null && question.Answer.Length > 0)
        {
            correctAnswer = string.Join(", ", question.Answer);
        }

        return new QuestionScoreViewModel
        {
            QuestionNumber = number,
            SectionTitle = sectionTitle,
            QuestionId = question.QuestionId ?? string.Empty,
            QuestionType = question.Type,
            QuestionStem = question.Stem,
            MaxScore = question.Score,
            ObtainedScore = score.ObtainedScore,
            IsCorrect = score.IsCorrect,
            IsAiJudged = question.IsAiJudge,
            IsEvaluated = question.IsAiEvaluated,
            UserAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0 
                ? string.Join(", ", question.UserAnswer) 
                : _localizationService["exam.result.no.answer"],
            CorrectAnswer = correctAnswer,
            AiFeedback = question.AiFeedback ?? string.Empty,
            SubQuestions = new ObservableCollection<QuestionScoreViewModel>()
        };
    }

    // 递归处理子问题，支持任意嵌套层级
    private void ProcessSubQuestions(
        QuestionScoreViewModel parentViewModel,
        Question parentQuestion,
        int depth)
    {
        if (parentQuestion.SubQuestions == null || parentQuestion.SubQuestions.Count == 0)
            return;
            
        for (int i = 0; i < parentQuestion.SubQuestions.Count; i++)
        {
            var subQuestion = parentQuestion.SubQuestions[i];
            string subQuestionId = subQuestion.QuestionId ?? $"{parentQuestion.QuestionId}_sub_{i}";
            
            string subCorrectAnswer = "";
            if (subQuestion.Answer != null && subQuestion.Answer.Length > 0)
            {
                subCorrectAnswer = string.Join(", ", subQuestion.Answer);
            }
            
            // 创建子问题的ViewModel
            var subViewModel = new QuestionScoreViewModel
            {
                QuestionNumber = i + 1,
                SectionTitle = parentViewModel.SectionTitle,
                QuestionId = subQuestionId,
                QuestionType = subQuestion.Type,
                QuestionStem = subQuestion.Stem,
                MaxScore = subQuestion.Score,
                ObtainedScore = subQuestion.ObtainedScore ?? 0,
                IsCorrect = Math.Abs((subQuestion.ObtainedScore ?? 0) - subQuestion.Score) < 0.001,
                IsAiJudged = subQuestion.IsAiJudge,
                IsEvaluated = subQuestion.IsAiEvaluated,
                UserAnswer = subQuestion.UserAnswer != null && subQuestion.UserAnswer.Length > 0 
                    ? string.Join(", ", subQuestion.UserAnswer) 
                    : _localizationService["exam.result.no.answer"],
                CorrectAnswer = subCorrectAnswer,
                AiFeedback = subQuestion.AiFeedback ?? string.Empty,
                SubQuestions = new ObservableCollection<QuestionScoreViewModel>()
            };
            
            // 如果子问题也是复合题，递归处理其子问题
            if (subQuestion.Type == QuestionTypes.Complex && 
                subQuestion.SubQuestions != null && 
                subQuestion.SubQuestions.Count > 0)
            {
                ProcessSubQuestions(subViewModel, subQuestion, depth + 1);
            }
            
            parentViewModel.SubQuestions.Add(subViewModel);
        }
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

        // 计算需要AI评分的问题
        var questionsToScore = new List<(ExaminationSection section, Question question, Question? parentQuestion)>();

        foreach (var section in Examination.ExaminationSections)
        {
            if (section.Questions == null) 
                continue;
            
            foreach (var question in section.Questions)
            {
                // 检查主问题
                if (IsQuestionNeedsAiScoring(question))
                {
                    questionsToScore.Add((section, question, null));
                }
                
                // 检查复合题的子问题
                if (question.Type == QuestionTypes.Complex && 
                    question.SubQuestions != null && 
                    question.SubQuestions.Count > 0)
                {
                    foreach (var subQuestion in question.SubQuestions)
                    {
                        if (IsQuestionNeedsAiScoring(subQuestion))
                        {
                            questionsToScore.Add((section, subQuestion, question));
                        }
                    }
                }
            }
        }

        if (questionsToScore.Count == 0) 
            return;

        // 处理每个问题
        for (int i = 0; i < questionsToScore.Count; i++)
        {
            var (section, question, parentQuestion) = questionsToScore[i];
            
            // 更新进度显示
            CurrentScoringQuestionProgress = $"{i + 1} / {questionsToScore.Count}";
            
            string cleanStem = question.Stem
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ");
            
            CurrentScoringQuestion = $"{section.Title}: {cleanStem.Substring(0, Math.Min(50, cleanStem.Length))}...";
            
            try
            {
                // 为AI评分生成提示
                string prompt = GenerateComprehensiveGradingPrompt(section, question, parentQuestion);
                
                _configService.AppStatistics.AddAiCallCount(_configService);
                // 发送到AI进行评分
                string? response = await AiTools.SendChatMessageAsync(
                    aiClient,
                    _configService.SystemConfig,
                    prompt);
                
                if (!string.IsNullOrEmpty(response))
                {
                    // 解析AI响应
                    var result = PromptTemplateManager.ParseAIResponse(response);
                    
                    // 用AI结果更新问题
                    question.ObtainedScore = result.Score;
                    question.AiFeedback = result.Feedback;
                    question.IsAiEvaluated = true; // 标记为已评估
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scoring question {question.QuestionId}: {ex.Message}");
                // 即使评分失败也标记为已评估
                question.IsAiEvaluated = true;
                question.ObtainedScore = 0.0;
                question.AiFeedback = "Error occurred during AI evaluation.";
            }
            
            // 更新进度
            AiScoringProgress = (double)(i + 1) / questionsToScore.Count * 100;
            
            // 重新计算分数并在每个问题后重新生成UI
            ScoreRecord.CalculateScores(Examination);
            ObtainedScore = ScoreRecord.ObtainedScore;
            ScorePercentage = TotalScore > 0 ? (ObtainedScore / TotalScore * 100) : 0;
            IsPassed = ScorePercentage >= 60;
            
            // 触发完整UI重新生成
            InitializeQuestionScores();
        }
    }

    // 生成包含所有相关参考资料的评分提示
    private string GenerateComprehensiveGradingPrompt(ExaminationSection section, Question question, Question? parentQuestion)
    {
        // 收集所有相关参考资料
        var allReferenceMaterials = new List<ReferenceMaterial>();
        
        // 添加考试级别的参考资料
        if (Examination.ExaminationMetadata.ReferenceMaterials != null)
        {
            allReferenceMaterials.AddRange(Examination.ExaminationMetadata.ReferenceMaterials);
        }
        
        // 添加章节级别的参考资料
        if (section.ReferenceMaterials != null)
        {
            allReferenceMaterials.AddRange(section.ReferenceMaterials);
        }
        
        // 如果是子问题，添加父问题的参考资料
        if (parentQuestion != null && parentQuestion.ReferenceMaterials != null)
        {
            allReferenceMaterials.AddRange(parentQuestion.ReferenceMaterials);
        }
        
        // 添加问题自身的参考资料
        if (question.ReferenceMaterials != null)
        {
            allReferenceMaterials.AddRange(question.ReferenceMaterials);
        }
        
        // 创建扩展的问题对象，包含收集到的所有参考资料
        var extendedQuestion = new Question
        {
            QuestionId = question.QuestionId,
            Type = question.Type,
            Stem = question.Stem,
            Options = question.Options,
            Score = question.Score,
            UserAnswer = question.UserAnswer,
            Answer = question.Answer,
            ReferenceAnswer = question.ReferenceAnswer,
            ReferenceMaterials = allReferenceMaterials.ToArray(),
            IsAiJudge = question.IsAiJudge,
            IgnoreSpace = question.IgnoreSpace,
            Commits = question.Commits
        };
        
        // 使用PromptTemplateManager生成包含所有参考资料的提示
        return PromptTemplateManager.GenerateGradingPrompt(
            extendedQuestion, 
            _configService.AppConfig.PromptGradingTemplate,
            true,
            _localizationService.CurrentLanguage);
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
        
            _configService.AppStatistics.AddAiCallCount(_configService);
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
    public ObservableCollection<QuestionScoreViewModel> SubQuestions { get; set; } = new();
}
