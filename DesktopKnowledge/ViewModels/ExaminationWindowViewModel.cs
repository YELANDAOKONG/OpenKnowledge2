using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopKnowledge.Services;
using DesktopKnowledge.Utils;
using DesktopKnowledge.Views;
using OpenKnowledge.Extensions;
using OpenKnowledge.Models;
using OpenKnowledge.Utilities;

namespace DesktopKnowledge.ViewModels;

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
    
    public event EventHandler<TimeConstraintEventArgs>? TimeConstraintViolated;
    public event EventHandler? ForceSubmitRequested;
    
    public Question? ParentQuestion { get; private set; }
    
    // This will be assigned by the view
    public Action SaveCurrentAnswer { get; set; } = () => { };
    
    private readonly LoggerService _logger;
    
    public ExaminationWindowViewModel(ConfigureService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _logger = App.GetWindowsLogger("ExaminationWindow");
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
    
    /// <summary>
    /// 检查当前考试时间是否满足时间限制要求
    /// </summary>
    /// <returns>时间检查结果</returns>
    public ExamTimeCheckResult CheckExaminationTimeConstraints()
    {
        try
        {
            if (Examination?.ExaminationMetadata == null)
                return new ExamTimeCheckResult { IsValid = true, CanSubmit = true };
            // 计算当前总考试时间
            long currentTotalTime = GetCurrentTotalExaminationTime();
            
            var metadata = Examination.ExaminationMetadata;
            
            // 检查最小时间限制
            if (metadata.MinimumExamTime.HasValue && metadata.MinimumExamTime.Value > 0)
            {
                if (currentTotalTime < metadata.MinimumExamTime.Value)
                {
                    return new ExamTimeCheckResult
                    {
                        IsValid = false,
                        CanSubmit = false,
                        Message = string.Format(
                            _localizationService?["exam.time.minimum.not.reached"] ?? "Insufficient examination time, minimum required: {0}, current: {1}",
                            TimeHelper.ToTimerString(metadata.MinimumExamTime.Value),
                            TimeHelper.ToTimerString(currentTotalTime)
                        )
                    };
                }
            }
            
            // 检查最大时间限制
            if (metadata.MaximumExamTime.HasValue && metadata.MaximumExamTime.Value > 0)
            {
                if (currentTotalTime >= metadata.MaximumExamTime.Value)
                {
                    return new ExamTimeCheckResult
                    {
                        IsValid = false,
                        CanSubmit = true,
                        ForceSubmit = true,
                        Message = _localizationService?["exam.time.maximum.exceeded"] ?? "Maximum examination time reached, system will auto-submit"
                    };
                }
            }
            
            return new ExamTimeCheckResult { IsValid = true, CanSubmit = true };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error checking examination time constraints: {ex.Message}");
            _logger.Trace($"Error checking examination time constraints: {ex.StackTrace}");
            return new ExamTimeCheckResult { IsValid = true, CanSubmit = true };
        }
    }
    
    /// <summary>
    /// 获取当前总考试时间（毫秒）
    /// </summary>
    private long GetCurrentTotalExaminationTime()
    {
        try
        {
            if (_configService?.AppData == null)
                return 0;
            
            if (!_configService.AppData.ExaminationTimer.HasValue)
                return _configService.AppData.AccumulatedExaminationTime;
            
            var currentSessionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _configService.AppData.ExaminationTimer.Value;
            return _configService.AppData.AccumulatedExaminationTime + currentSessionTime;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error getting current total examination time: {ex.Message}");
            _logger.Trace($"Error getting current total examination time: {ex.StackTrace}");
            return 0;
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
            var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson!);
            
            if (examinationCopy != null)
            {
                // Update the examination in app data
                _configService.AppData.CurrentExamination = examinationCopy;
                _configService.AppData.IsInExamination = true;
                
                // The timer will be managed in the view to properly accumulate time
                await _configService.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Silently log the error
            _logger.Error($"Error saving progress silently: {ex.Message}");
            _logger.Trace($"Error saving progress silently: {ex.StackTrace}");
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
            var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson!);
            
            if (examinationCopy != null)
            {
                // Update the examination in app data
                _configService.AppData.CurrentExamination = examinationCopy;
                _configService.AppData.IsInExamination = true;
                
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
    
    public async Task SubmitExamination(bool forceSubmit = false)
    {
        if (Examination == null) return;

        try
        {
            // 如果不是强制提交，检查时间约束
            if (!forceSubmit)
            {
                var timeCheck = CheckExaminationTimeConstraints();
                if (!timeCheck.CanSubmit)
                {
                    // 通过事件通知UI显示错误信息
                    TimeConstraintViolated?.Invoke(this, new TimeConstraintEventArgs(timeCheck.Message));
                    return; // 重要：这里直接返回，不继续执行提交逻辑
                }
            }

            // 只有通过时间检查或强制提交时才执行以下逻辑
            
            // Save final answers
            SaveCurrentAnswer();
            
            // 计算考试时间并累加到统计数据
            long accumulatedTime = 0;
            if (_configService?.AppData != null)
            {
                // 保存当前会话的时间
                if (_configService.AppData.ExaminationTimer.HasValue)
                {
                    var currentSessionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _configService.AppData.ExaminationTimer.Value;
                    _configService.AppData.AccumulatedExaminationTime += currentSessionTime;
                }
                
                accumulatedTime = _configService.AppData.AccumulatedExaminationTime;
                
                // 将累计考试时间添加到统计数据
                if (accumulatedTime > 0)
                {
                    _configService.AppStatistics?.AddExaminationTime(_configService, accumulatedTime);
                }
            
                // 重置计时器和累计时间
                _configService.AppData.ExaminationTimer = null;
                _configService.AppData.AccumulatedExaminationTime = 0;
            }
            
            // 累加统计计数
            var config = App.GetService<ConfigureService>();
            config?.AppStatistics?.AddSubmitExaminationCount(config);

            // Calculate scores
            var scoreRecord = new ScoreRecord
            {
                ExamId = Examination.ExaminationMetadata?.ExamId ?? string.Empty,
                ExamTitle = Examination.ExaminationMetadata?.Title ?? "Unknown",
                UserName = _configService?.AppConfig?.UserName ?? "Unknown"
            };

            // Calculate scores
            scoreRecord.CalculateScores(Examination);

            // Create a deep copy to avoid issues with Options
            var examinationJson = ExaminationSerializer.SerializeToJson(Examination);
            var examinationCopy = ExaminationSerializer.DeserializeFromJson(examinationJson!);

            if (examinationCopy != null && _configService?.AppData != null)
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
        catch (Exception ex)
        {
            _logger.Error($"Error submitting examination: {ex.Message}");
            _logger.Trace($"Error submitting examination: {ex.StackTrace}");
            // 通知UI显示错误，但不重置考试状态
            TimeConstraintViolated?.Invoke(this, new TimeConstraintEventArgs($"Error submitting examination: {ex.Message}"));
        }
    }

    
    public void BackToMain()
    {
        // Save progress before exiting
        SaveCurrentAnswer();
        _ = SaveProgressSilently();
        
        WindowCloseRequested?.Invoke(this, EventArgs.Empty);
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

public class ExamTimeCheckResult
{
    public bool IsValid { get; set; }
    public bool CanSubmit { get; set; }
    public bool ForceSubmit { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TimeConstraintEventArgs : EventArgs
{
    public string Message { get; }
    
    public TimeConstraintEventArgs(string message)
    {
        Message = message;
    }
}