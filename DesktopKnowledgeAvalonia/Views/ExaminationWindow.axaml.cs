using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;
using LibraryOpenKnowledge.Extensions;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;

namespace DesktopKnowledgeAvalonia.Views;

public partial class ExaminationWindow : AppWindowBase
{
    private readonly ExaminationWindowViewModel _viewModel;
    private readonly LocalizationService _localizationService;
    private readonly ConfigureService _configService;
    private readonly Dictionary<string, TextBox> _textAnswers = new();
    private readonly Dictionary<string, object> _choiceAnswers = new();
    private System.Timers.Timer _autoSaveTimer;
    private System.Timers.Timer _uiUpdateTimer;
    private long _sessionStartTime; // Track when this session started
    private bool _isSubmitting = false;

    private LoggerService _logger;
    
    // Constructor with optional parameters for loading examinations
    public ExaminationWindow(string? filePath = null, bool force = false)
    {
        try
        {
            InitializeComponent();
        
            // Get services
            _localizationService = App.GetService<LocalizationService>();
            _configService = App.GetService<ConfigureService>();
            _viewModel = new ExaminationWindowViewModel(_configService, _localizationService);
            _logger = App.GetWindowsLogger("ExaminationWindow");
        
            // Set DataContext for convenience
            DataContext = _viewModel;
        
            // Subscribe to view model events
            _viewModel.ExaminationLoaded += OnExaminationLoaded;
            _viewModel.QuestionChanged += OnQuestionChanged;
            _viewModel.ProgressUpdated += OnProgressUpdated;
            _viewModel.WindowCloseRequested += (s, e) => Close();
            _viewModel.TimeConstraintViolated += OnTimeConstraintViolated;
            _viewModel.ForceSubmitRequested += OnForceSubmitRequested;
        
            // Initialize UI text
            InitializeUI();
        
            // Set up event handlers
            SetupEventHandlers();
        
            // Load examination from file or current examination
            LoadExamination(filePath, force);
        
            // Initialize timers
            InitializeTimers();
        
            // Assign the save method to ViewModel's delegate
            _viewModel.SaveCurrentAnswer = SaveCurrentAnswer;
        }
        catch (Exception ex)
        {
            var logger = App.GetWindowsLogger("ExaminationWindow");
            logger.Error($"Error initializing examination window: {ex.Message}");
            logger.Trace($"Error initializing examination window: {ex.StackTrace}");
            throw;
        }
    }
    
    private void InitializeUI()
    {
        // Set static text elements
        SectionsHeader.Text = _localizationService["exam.sections.questions"];
        SubmitButtonText.Text = _localizationService["exam.submit"];
        PrevButton.Content = _localizationService["exam.prev"];
        NextButton.Content = _localizationService["exam.next"];
        ProgressText.Text = _localizationService["exam.progress"];
    }
    
    private void SetupEventHandlers()
    {
        // Button click handlers
        BackButton.Click += async (s, e) => 
        {
            SaveCurrentAnswer();
            SaveAccumulatedTime(); // Save the accumulated time when navigating back
            await _viewModel.SaveProgressSilently();
            Close();
        };
        
        SaveButton.Click += async (s, e) => await _viewModel.SaveProgress();
        SubmitButton.Click += async (s, e) => await SubmitExamination();
        PrevButton.Click += (s, e) => _viewModel.NavigatePrevious();
        NextButton.Click += (s, e) => _viewModel.NavigateNext();
    }
    
    private void InitializeTimers()
    {
        try
        {
            // UI update timer (0.5 seconds)
            _uiUpdateTimer = new System.Timers.Timer(500);
            _uiUpdateTimer.Elapsed += (s, e) => 
            {
                Dispatcher.UIThread.InvokeAsync(() => 
                {
                    try
                    {
                        if (_viewModel?.Examination != null && !_isSubmitting)
                        {
                            UpdateTimeDisplay();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in UI update timer: {ex.Message}");
                        _logger.Trace($"Error in UI update timer: {ex.Message}");
                    }
                });
            };
            _uiUpdateTimer.Start();
        
            // Auto-save timer (5 seconds)
            _autoSaveTimer = new System.Timers.Timer(5000);
            _autoSaveTimer.Elapsed += async (s, e) => 
            {
                await Dispatcher.UIThread.InvokeAsync(async () => 
                {
                    try
                    {
                        if (_viewModel?.Examination != null && 
                            _configService?.AppData?.IsInExamination == true && 
                            !_isSubmitting)
                        {
                            // First save current answer
                            SaveCurrentAnswer();
                            // Also save accumulated time
                            SaveAccumulatedTime();
                            // Then silently save to disk
                            await _viewModel.SaveProgressSilently();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in auto-save timer: {ex.Message}");
                        _logger.Trace($"Error in auto-save timer: {ex.StackTrace}");
                    }
                });
            };
            _autoSaveTimer.Start();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error initializing timers: {ex.Message}");
            _logger.Trace($"Error initializing timers: {ex.StackTrace}");
        }
    }
    
    private void UpdateTimeDisplay()
    {
        try
        {
            if (_configService?.AppData?.ExaminationTimer.HasValue == true)
            {
                // Calculate time spent in current session
                var currentSessionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _configService.AppData.ExaminationTimer.Value;
            
                // Add to accumulated time from previous sessions
                var totalTimeMs = _configService.AppData.AccumulatedExaminationTime + currentSessionTime;
            
                // Convert to TimeSpan for formatting
                var timeSpan = TimeSpan.FromMilliseconds(totalTimeMs);
                string timeFormat = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            
                if (_viewModel != null)
                {
                    _viewModel.TimeRemaining = timeFormat;
                }
            
                if (TimeRemainingText != null && _localizationService != null)
                {
                    TimeRemainingText.Text = string.Format(_localizationService["exam.time"], timeFormat);
                }
            
                // 检查时间约束（只有在考试进行中且窗口可见时）
                if (_viewModel?.IsWindowVisible == true && !_isSubmitting)
                {
                    CheckTimeConstraints(totalTimeMs);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating time display: {ex.Message}");
            _logger.Trace($"Error updating time display: {ex.StackTrace}");
        }
    }
    
    private void CheckTimeConstraints(long currentTotalTimeMs)
    {
        try
        {
            if (_viewModel?.Examination?.ExaminationMetadata == null || _isSubmitting) return;
            var metadata = _viewModel.Examination.ExaminationMetadata;
        
            // 检查是否超过最大时间限制
            if (metadata.MaximumExamTime.HasValue && 
                metadata.MaximumExamTime.Value > 0 && 
                currentTotalTimeMs >= metadata.MaximumExamTime.Value)
            {
                // 设置提交状态，防止重复执行
                _isSubmitting = true;
            
                // 停止定时器
                _uiUpdateTimer?.Stop();
                _autoSaveTimer?.Stop();
            
                // 显示强制提交消息并自动提交
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        await ShowStatusMessage(_localizationService?["exam.time.force.submit"] ?? "Time is up, auto-submitting examination...", true);
                    
                        await Task.Delay(2000); // 显示2秒提示
                    
                        // 强制提交
                        await _viewModel.SubmitExamination(forceSubmit: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error during force submit: {ex.Message}");
                        _logger.Trace($"Error during force submit: {ex.StackTrace}");
                        _isSubmitting = false; // 重置状态
                        RestartTimers(); // 重启定时器
                        UpdateUIState(); // 更新UI状态
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error checking time constraints: {ex.Message}");
            _logger.Trace($"Error checking time constraints: {ex.StackTrace}");
        }
    }
    
    private void LoadExamination(string? filePath, bool force)
    {
        Examination? examination = null;
        
        // Check if there's an ongoing examination
        bool hasOngoingExam = _configService.AppData.IsInExamination && _configService.AppData.CurrentExamination != null;
        
        if (filePath == null)
        {
            // No file specified, use current examination if available
            if (hasOngoingExam)
            {
                examination = _configService.AppData.CurrentExamination;
                
                // Start a new session with the current timestamp
                _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                _sessionStartTime = _configService.AppData.ExaminationTimer.Value;
                _ = _configService.SaveChangesAsync();
            }
            else
            {
                // No ongoing exam and no file path provided
                ShowError("No examination in progress and no file specified.");
                return;
            }
        }
        else
        {
            // File path specified
            if (hasOngoingExam && !force)
            {
                // Continue with ongoing exam
                examination = _configService.AppData.CurrentExamination;
                
                // Start a new session with the current timestamp
                _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                _sessionStartTime = _configService.AppData.ExaminationTimer.Value;
                _ = _configService.SaveChangesAsync();
            }
            else
            {
                // Load from file
                examination = ExaminationSerializer.DeserializeFromFile(filePath);
                
                if (examination != null)
                {
                    // Set as current examination
                    _configService.AppData.CurrentExamination = examination;
                    _configService.AppData.IsInExamination = true;
                    
                    // Initialize timers for a new examination
                    _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    _sessionStartTime = _configService.AppData.ExaminationTimer.Value;
                    
                    // Reset accumulated time for a new examination
                    _configService.AppData.AccumulatedExaminationTime = 0;
                    
                    _ = _configService.SaveChangesAsync();
                }
                else
                {
                    // Failed to load
                    ShowError($"Failed to load examination from file: {filePath}");
                    return;
                }
            }
        }
        
        _viewModel.Examination = examination;
        _viewModel.Initialize();
    }
    
    private async void ShowError(string message)
    {
        StatusMessageText.Text = message;
        StatusOverlay.IsVisible = true;
        StatusProgressBar.IsVisible = false;
        
        await Task.Delay(3000);
        StatusOverlay.IsVisible = false;
    }
    
    private void OnExaminationLoaded(object? sender, EventArgs e)
    {
        if (_viewModel.Examination == null) return;
        
        // Set exam title and subject
        ExamTitle.Text = _viewModel.Examination.ExaminationMetadata.Title;
        ExamSubject.Text = _viewModel.Examination.ExaminationMetadata.Subject;
        
        // Create sections and questions UI
        BuildSectionsUI();
        
        // Update current question
        OnQuestionChanged(sender, e);
        
        // Update progress
        OnProgressUpdated(sender, e);
        
        // Update time display
        UpdateTimeDisplay();
    }
    
    // Save the accumulated time to the config
    private void SaveAccumulatedTime()
    {
        if (_configService.AppData.ExaminationTimer.HasValue)
        {
            // Calculate time spent in current session
            var currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var sessionTime = currentTime - _configService.AppData.ExaminationTimer.Value;
            
            // Add to accumulated time
            _configService.AppData.AccumulatedExaminationTime += sessionTime;
            
            // Reset the timer for the next interval
            _configService.AppData.ExaminationTimer = currentTime;
        }
    }
    
    private void BuildSectionsUI()
    {
        SectionsPanel.Children.Clear();
        
        if (_viewModel.Examination == null) return;
        
        for (int sectionIndex = 0; sectionIndex < _viewModel.Examination.ExaminationSections.Length; sectionIndex++)
        {
            var section = _viewModel.Examination.ExaminationSections[sectionIndex];
            
            // Create expander for this section
            var expander = new Expander
            {
                Header = section.Title,
                IsExpanded = sectionIndex == _viewModel.CurrentSectionIndex, // Only expand current section
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            // Create scroll viewer for questions to enable horizontal scrolling
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            
            // Create stack panel for questions
            var questionsPanel = new StackPanel();
            
            // Create question buttons
            if (section.Questions != null)
            {
                for (int questionIndex = 0; questionIndex < section.Questions.Length; questionIndex++)
                {
                    var question = section.Questions[questionIndex];
                    var finalSectionIndex = sectionIndex;
                    var finalQuestionIndex = questionIndex;
                    
                    // Create button for this question
                    var button = new Button
                    {
                        Classes = { "Subtle" },
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 2)
                    };
                    
                    // Set background based on whether question has been answered
                    bool hasAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0;
                    button.Background = hasAnswer 
                        ? new SolidColorBrush(Color.Parse("#22569AFF")) 
                        : new SolidColorBrush(Colors.Transparent);
                    
                    // Create grid for button content
                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    
                    // Add question number
                    var numberText = new TextBlock
                    {
                        Text = (questionIndex + 1).ToString(),
                        Margin = new Thickness(0, 0, 8, 0)
                    };
                    Grid.SetColumn(numberText, 0);
                    grid.Children.Add(numberText);
                    
                    // Add question title - with truncated text (first 40 chars max)
                    string truncatedStem = TruncateWithEllipsis(question.Stem, 40);
                    var titleText = new TextBlock
                    {
                        Text = truncatedStem,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxLines = 2 // Limit to 2 lines max
                    };
                    Grid.SetColumn(titleText, 1);
                    grid.Children.Add(titleText);
                    
                    // Set full text as tooltip for hover
                    ToolTip.SetTip(button, question.Stem);
                    
                    // Add check icon if answered
                    var checkIcon = new PathIcon
                    {
                        Data = StreamGeometry.Parse("M10.09 13.5 7.02 10.4a.75.75 0 0 0-1.06 1.06l3.5 3.5a1 1 0 0 0 1.42 0l7.5-7.5a.75.75 0 1 0-1.06-1.06l-7.23 7.1Z"),
                        Width = 12,
                        Height = 12,
                        Margin = new Thickness(8, 0, 0, 0),
                        IsVisible = hasAnswer
                    };
                    Grid.SetColumn(checkIcon, 2);
                    grid.Children.Add(checkIcon);
                    
                    button.Content = grid;
                    
                    // Set click handler
                    button.Click += (s, e) => _viewModel.NavigateToQuestion(finalSectionIndex, finalQuestionIndex);
                    
                    questionsPanel.Children.Add(button);
                }
            }
            
            scrollViewer.Content = questionsPanel;
            expander.Content = scrollViewer;
            SectionsPanel.Children.Add(expander);
        }
    }
    
    // Helper method to truncate text with ellipsis
    private string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
            
        return text.Substring(0, maxLength) + "...";
    }
    
    private void OnQuestionChanged(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentQuestion == null) return;
        
        // Ensure question has valid Options for judgement questions
        EnsureQuestionOptions();
        
        // Update question info
        QuestionTypeText.Text = _viewModel.GetLocalizedQuestionType(_viewModel.CurrentQuestion.Type);
        QuestionScoreText.Text = string.Format(_localizationService["exam.points"], _viewModel.CurrentQuestion.Score);
        QuestionNumberText.Text = string.Format(_localizationService["exam.question"], _viewModel.CurrentQuestionIndex + 1);
        QuestionStemText.Text = _viewModel.CurrentQuestion.Stem;
        
        // Update navigation info
        CurrentQuestionText.Text = (_viewModel.CurrentQuestionIndex + 1).ToString();
        TotalQuestionsText.Text = _viewModel.CurrentSection?.Questions?.Length.ToString() ?? "0";
        
        // Enable/disable navigation buttons
        PrevButton.IsEnabled = _viewModel.CurrentSectionIndex > 0 || _viewModel.CurrentQuestionIndex > 0;
        
        bool hasNextQuestion = false;
        if (_viewModel.CurrentSection?.Questions != null)
        {
            hasNextQuestion = _viewModel.CurrentQuestionIndex < _viewModel.CurrentSection.Questions.Length - 1;
        }
        bool hasNextSection = _viewModel.CurrentSectionIndex < _viewModel.Examination.ExaminationSections.Length - 1;
        NextButton.IsEnabled = hasNextQuestion || hasNextSection;
        
        // 修改: 检查所有级别的参考资料，而不仅仅是当前问题的参考资料
        bool hasExamReferenceMaterials = _viewModel.Examination?.ExaminationMetadata?.ReferenceMaterials != null && 
                                        _viewModel.Examination.ExaminationMetadata.ReferenceMaterials.Length > 0;
        bool hasSectionReferenceMaterials = _viewModel.CurrentSection?.ReferenceMaterials != null && 
                                            _viewModel.CurrentSection.ReferenceMaterials.Length > 0;
        bool hasQuestionReferenceMaterials = _viewModel.CurrentQuestion.ReferenceMaterials != null && 
                                            _viewModel.CurrentQuestion.ReferenceMaterials.Length > 0;
                                            
        // 查找是否有父问题的参考资料
        Question? parentQuestion = FindParentQuestion();
        bool hasParentReferenceMaterials = parentQuestion?.ReferenceMaterials != null && 
                                           parentQuestion.ReferenceMaterials.Length > 0;
                                            
        bool hasAnyReferenceMaterials = hasExamReferenceMaterials || hasSectionReferenceMaterials || 
                                       hasQuestionReferenceMaterials || hasParentReferenceMaterials;
                                        
        // 如果有任何级别的参考资料，就显示参考资料区域
        ReferenceMaterialsExpander.IsVisible = hasAnyReferenceMaterials;
        
        // 修改: 移除参考资料的总标题，避免形成二级嵌套
        // ReferenceMaterialsExpander.Header = _localizationService["exam.reference"];
        ReferenceMaterialsExpander.Header = string.Empty;
        
        if (hasAnyReferenceMaterials)
        {
            BuildReferenceMaterialsUI();
        }
        
        // 更新章节展开状态
        for (int i = 0; i < SectionsPanel.Children.Count; i++)
        {
            if (SectionsPanel.Children[i] is Expander expander)
            {
                expander.IsExpanded = i == _viewModel.CurrentSectionIndex;
            }
        }
        
        BuildAnswerUI();
    }

    
    // Helper method to ensure judgment questions have proper Options
    private void EnsureQuestionOptions()
    {
        if (_viewModel.CurrentQuestion == null) return;
        
        // For judgment questions, ensure Options are set properly
        if (_viewModel.CurrentQuestion.Type == QuestionTypes.Judgment)
        {
            if (_viewModel.CurrentQuestion.Options == null || _viewModel.CurrentQuestion.Options.Count < 2)
            {
                // Create default True/False options if none exist
                _viewModel.CurrentQuestion.Options = new List<(string, string)>
                {
                    ("True", "True"),
                    ("False", "False")
                }.ToOptionList();
            }
        }
    }
    
    // 在 ExaminationResultWindow.axaml.cs 中修改
    private void BuildReferenceMaterialsUI()
    {
        ReferenceMaterialsPanel.Children.Clear();
    
        // Restore the main header for the single expander
        ReferenceMaterialsExpander.Header = _localizationService["exam.reference"];
    
        // Add examination level materials
        AddReferenceSection(
            _viewModel.Examination?.ExaminationMetadata?.ReferenceMaterials,
            _localizationService["exam.reference.examination"]);
    
        // Add section level materials
        AddReferenceSection(
            _viewModel.CurrentSection?.ReferenceMaterials,
            _localizationService["exam.reference.section"]);
    
        // Add question level materials
        AddReferenceSection(
            _viewModel.CurrentQuestion?.ReferenceMaterials,
            _localizationService["exam.reference.question"]);
    
        // Add parent question materials (if current question is a sub-question)
        Question? parentQuestion = FindParentQuestion();
        if (parentQuestion != null)
        {
            AddReferenceSection(
                parentQuestion.ReferenceMaterials,
                _localizationService["exam.reference.parent"]);
        }
    }

    // 添加参考资料区块
    private void AddReferenceSection(ReferenceMaterial[]? materials, string headerText)
    {
        if (materials == null || materials.Length == 0)
            return;
            
        bool hasContent = false;
        
        // 创建面板来容纳参考资料
        var sectionPanel = new StackPanel { Spacing = 5 };
        
        // 处理文本参考资料
        foreach (var material in materials)
        {
            if (material.Materials != null && material.Materials.Length > 0)
            {
                foreach (var text in material.Materials)
                {
                    var textBlock = new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 3)
                    };
                    sectionPanel.Children.Add(textBlock);
                    hasContent = true;
                }
            }
            
            // 处理其他类型的参考资料
            // 这里可以添加对文档、图像、音频、视频等的处理
        }
        
        if (hasContent)
        {
            // 创建带标题的展开器作为一级元素
            var expander = new Expander
            {
                Header = headerText,
                IsExpanded = true,
                Margin = new Thickness(0, 5, 0, 5)
            };
            expander.Content = sectionPanel;
            
            // 添加到主面板
            ReferenceMaterialsPanel.Children.Add(expander);
        }
    }

    // 查找当前问题的父问题
    private Question? FindParentQuestion()
    {
        if (_viewModel.CurrentSection?.Questions == null || _viewModel.CurrentQuestion == null)
            return null;
            
        foreach (var question in _viewModel.CurrentSection.Questions)
        {
            if (question.Type == QuestionTypes.Complex && 
                question.SubQuestions != null)
            {
                foreach (var subQuestion in question.SubQuestions)
                {
                    if (subQuestion == _viewModel.CurrentQuestion || 
                        (subQuestion.QuestionId != null && 
                         _viewModel.CurrentQuestion.QuestionId != null && 
                         subQuestion.QuestionId == _viewModel.CurrentQuestion.QuestionId))
                    {
                        return question;
                    }
                }
            }
        }
        
        return null;
    }



        
    private void BuildAnswerUI()
    {
        AnswerContainer.Children.Clear();
    
        if (_viewModel.CurrentQuestion == null) return;
    
        string questionId = _viewModel.CurrentQuestion.QuestionId ?? Guid.NewGuid().ToString();
    
        switch (_viewModel.CurrentQuestion.Type)
        {
            case QuestionTypes.SingleChoice:
                CreateSingleChoiceUI(questionId);
                break;
            
            case QuestionTypes.MultipleChoice:
                CreateMultipleChoiceUI(questionId);
                break;
            
            case QuestionTypes.Judgment:
                CreateJudgmentUI(questionId);
                break;
            
            case QuestionTypes.Complex:
                CreateComplexQuestionUI(questionId);
                break;
            
            case QuestionTypes.FillInTheBlank:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Essay:
            case QuestionTypes.Math:
            case QuestionTypes.Calculation:
            case QuestionTypes.Other:
                CreateTextAnswerUI(questionId);
                break;
        }
    }

    
    private void CreateSingleChoiceUI(string questionId)
    {
        if (_viewModel.CurrentQuestion?.Options == null || _viewModel.CurrentQuestion.Options.Count == 0) return;
        
        var radioButtons = new List<RadioButton>();
        
        // Get letter prefixes for options (A, B, C, etc.)
        var letters = GetOptionLetters(_viewModel.CurrentQuestion.Options.Count);
        
        for (int i = 0; i < _viewModel.CurrentQuestion.Options.Count; i++)
        {
            var option = _viewModel.CurrentQuestion.Options[i];
            var letter = letters[i];
            
            var radioButton = new RadioButton
            {
                Content = $"{letter}. {option.Text}",
                GroupName = $"SingleChoice_{questionId}",
                Margin = new Thickness(0, 5),
                Tag = option.Id // Store the option ID in the Tag
            };
            
            // Check if this option is selected
            if (_viewModel.CurrentQuestion.UserAnswer != null && 
                _viewModel.CurrentQuestion.UserAnswer.Contains(option.Id))
            {
                radioButton.IsChecked = true;
            }
            
            // Set handler
            string optionId = option.Id;
            radioButton.Checked += (s, e) => 
            {
                if (_viewModel.CurrentQuestion != null)
                {
                    _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                    _viewModel.CurrentQuestion.UserAnswer = new[] { optionId };
                    _viewModel.UpdateProgress();
                }
            };
            
            radioButtons.Add(radioButton);
            AnswerContainer.Children.Add(radioButton);
        }
        
        _choiceAnswers[questionId] = radioButtons;
    }
    
    private void CreateMultipleChoiceUI(string questionId)
    {
        if (_viewModel.CurrentQuestion?.Options == null || _viewModel.CurrentQuestion.Options.Count == 0) return;
        
        var checkBoxes = new List<CheckBox>();
        
        // Get letter prefixes for options (A, B, C, etc.)
        var letters = GetOptionLetters(_viewModel.CurrentQuestion.Options.Count);
        
        for (int i = 0; i < _viewModel.CurrentQuestion.Options.Count; i++)
        {
            var option = _viewModel.CurrentQuestion.Options[i];
            var letter = letters[i];
            
            var checkBox = new CheckBox
            {
                Content = $"{letter}. {option.Text}",
                Margin = new Thickness(0, 5),
                Tag = option.Id // Store the option ID in the Tag
            };
            
            // Check if this option is selected
            if (_viewModel.CurrentQuestion.UserAnswer != null && 
                _viewModel.CurrentQuestion.UserAnswer.Contains(option.Id))
            {
                checkBox.IsChecked = true;
            }
            
            // Set handler
            checkBox.Checked += (s, e) => UpdateMultipleChoiceAnswer(questionId);
            checkBox.Unchecked += (s, e) => UpdateMultipleChoiceAnswer(questionId);
            
            checkBoxes.Add(checkBox);
            AnswerContainer.Children.Add(checkBox);
        }
        
        _choiceAnswers[questionId] = checkBoxes;
    }
    
    private void UpdateMultipleChoiceAnswer(string questionId)
    {
        if (_viewModel.CurrentQuestion == null || !_choiceAnswers.ContainsKey(questionId)) return;
        _configService.AppStatistics.AddQuestionInteractionCount(_configService);
        
        var checkBoxes = _choiceAnswers[questionId] as List<CheckBox>;
        if (checkBoxes == null) return;
        
        var selectedOptions = new List<string>();
        
        foreach (var checkBox in checkBoxes)
        {
            if (checkBox.IsChecked == true && checkBox.Tag is string optionId)
            {
                selectedOptions.Add(optionId);
            }
        }
        
        _viewModel.CurrentQuestion.UserAnswer = selectedOptions.ToArray();
        _viewModel.UpdateProgress();
    }
    
    private void CreateJudgmentUI(string questionId)
    {
        if (_viewModel.CurrentQuestion?.Options == null || _viewModel.CurrentQuestion.Options.Count < 2) return;
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        
        // Get options (should be True/False)
        var trueOption = _viewModel.CurrentQuestion.Options[0];
        var falseOption = _viewModel.CurrentQuestion.Options[1];
        
        var trueRadio = new RadioButton
        {
            Content = "T. " + trueOption.Text,
            GroupName = $"Judgment_{questionId}",
            Tag = trueOption.Id
        };
        
        var falseRadio = new RadioButton
        {
            Content = "F. " + falseOption.Text,
            GroupName = $"Judgment_{questionId}",
            Tag = falseOption.Id
        };
        
        // Set initial state
        if (_viewModel.CurrentQuestion.UserAnswer != null && _viewModel.CurrentQuestion.UserAnswer.Length > 0)
        {
            string answer = _viewModel.CurrentQuestion.UserAnswer[0];
            trueRadio.IsChecked = answer.Equals(trueOption.Id, StringComparison.OrdinalIgnoreCase);
            falseRadio.IsChecked = answer.Equals(falseOption.Id, StringComparison.OrdinalIgnoreCase);
        }
        
        // Set handlers
        trueRadio.Checked += (s, e) => 
        {
            if (_viewModel.CurrentQuestion != null && trueRadio.Tag is string optionId)
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                _viewModel.CurrentQuestion.UserAnswer = new[] { optionId };
                _viewModel.UpdateProgress();
            }
        };
        
        falseRadio.Checked += (s, e) => 
        {
            if (_viewModel.CurrentQuestion != null && falseRadio.Tag is string optionId)
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                _viewModel.CurrentQuestion.UserAnswer = new[] { optionId };
                _viewModel.UpdateProgress();
            }
        };
        
        panel.Children.Add(trueRadio);
        panel.Children.Add(falseRadio);
        
        AnswerContainer.Children.Add(panel);
        
        var radioButtons = new List<RadioButton> { trueRadio, falseRadio };
        _choiceAnswers[questionId] = radioButtons;
    }

    // 创建复合题UI
    private void CreateComplexQuestionUI(string questionId)
    {
        if (_viewModel.CurrentQuestion?.SubQuestions == null || _viewModel.CurrentQuestion.SubQuestions.Count == 0) 
        {
            // 如果没有子问题，则作为普通文本题处理
            CreateTextAnswerUI(questionId);
            return;
        }
        
        // 创建子问题容器
        var subQuestionsContainer = new StackPanel
        {
            Spacing = 10
        };
        
        // 为每个子问题创建UI
        for (int i = 0; i < _viewModel.CurrentQuestion.SubQuestions.Count; i++)
        {
            var subQuestion = _viewModel.CurrentQuestion.SubQuestions[i];
            string subQuestionId = subQuestion.QuestionId ?? $"{questionId}_sub_{i}";
            
            // 创建子问题容器
            var subQuestionContainer = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#10FFFFFF")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5)
            };
            
            var subQuestionPanel = new StackPanel
            {
                Spacing = 5
            };
            
            // 添加子问题题干
            var stemText = new TextBlock
            {
                Text = subQuestion.Stem,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5)
            };
            subQuestionPanel.Children.Add(stemText);
            
            // 根据子问题类型添加答题UI
            switch (subQuestion.Type)
            {
                case QuestionTypes.SingleChoice:
                    CreateSubQuestionSingleChoiceUI(subQuestionPanel, subQuestion, subQuestionId);
                    break;
                    
                case QuestionTypes.MultipleChoice:
                    CreateSubQuestionMultipleChoiceUI(subQuestionPanel, subQuestion, subQuestionId);
                    break;
                    
                case QuestionTypes.Judgment:
                    CreateSubQuestionJudgmentUI(subQuestionPanel, subQuestion, subQuestionId);
                    break;
                    
                default:
                    CreateSubQuestionTextAnswerUI(subQuestionPanel, subQuestion, subQuestionId);
                    break;
            }
            
            subQuestionContainer.Child = subQuestionPanel;
            subQuestionsContainer.Children.Add(subQuestionContainer);
        }
        
        AnswerContainer.Children.Add(subQuestionsContainer);
    }

    // 子问题UI生成的辅助方法 - 单选题
    private void CreateSubQuestionSingleChoiceUI(StackPanel container, Question subQuestion, string subQuestionId)
    {
        if (subQuestion.Options == null || subQuestion.Options.Count == 0) return;
        
        var radioButtons = new List<RadioButton>();
        var letters = GetOptionLetters(subQuestion.Options.Count);
        
        for (int i = 0; i < subQuestion.Options.Count; i++)
        {
            var option = subQuestion.Options[i];
            var letter = letters[i];
            
            var radioButton = new RadioButton
            {
                Content = $"{letter}. {option.Text}",
                GroupName = $"SingleChoice_{subQuestionId}",
                Margin = new Thickness(0, 5),
                Tag = option.Id
            };
            
            // 检查是否已选择
            if (subQuestion.UserAnswer != null && 
                subQuestion.UserAnswer.Contains(option.Id))
            {
                radioButton.IsChecked = true;
            }
            
            // 设置事件处理
            string optionId = option.Id;
            radioButton.Checked += (s, e) => 
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                subQuestion.UserAnswer = new[] { optionId };
                _viewModel.UpdateProgress();
            };
            
            radioButtons.Add(radioButton);
            container.Children.Add(radioButton);
        }
        
        // 存储单选按钮以备后用
        _choiceAnswers[subQuestionId] = radioButtons;
    }

    // 子问题UI生成的辅助方法 - 多选题
    private void CreateSubQuestionMultipleChoiceUI(StackPanel container, Question subQuestion, string subQuestionId)
    {
        if (subQuestion.Options == null || subQuestion.Options.Count == 0) return;
        
        var checkBoxes = new List<CheckBox>();
        var letters = GetOptionLetters(subQuestion.Options.Count);
        
        for (int i = 0; i < subQuestion.Options.Count; i++)
        {
            var option = subQuestion.Options[i];
            var letter = letters[i];
            
            var checkBox = new CheckBox
            {
                Content = $"{letter}. {option.Text}",
                Margin = new Thickness(0, 5),
                Tag = option.Id
            };
            
            // 检查是否已选择
            if (subQuestion.UserAnswer != null && 
                subQuestion.UserAnswer.Contains(option.Id))
            {
                checkBox.IsChecked = true;
            }
            
            // 设置事件处理
            checkBox.Checked += (s, e) => UpdateSubQuestionMultipleChoiceAnswer(subQuestionId, subQuestion);
            checkBox.Unchecked += (s, e) => UpdateSubQuestionMultipleChoiceAnswer(subQuestionId, subQuestion);
            
            checkBoxes.Add(checkBox);
            container.Children.Add(checkBox);
        }
        
        _choiceAnswers[subQuestionId] = checkBoxes;
    }

    private void UpdateSubQuestionMultipleChoiceAnswer(string subQuestionId, Question subQuestion)
    {
        if (!_choiceAnswers.ContainsKey(subQuestionId)) return;
        _configService.AppStatistics.AddQuestionInteractionCount(_configService);
        
        var checkBoxes = _choiceAnswers[subQuestionId] as List<CheckBox>;
        if (checkBoxes == null) return;
        
        var selectedOptions = new List<string>();
        
        foreach (var checkBox in checkBoxes)
        {
            if (checkBox.IsChecked == true && checkBox.Tag is string optionId)
            {
                selectedOptions.Add(optionId);
            }
        }
        
        subQuestion.UserAnswer = selectedOptions.ToArray();
        _viewModel.UpdateProgress();
    }

    // 子问题UI生成的辅助方法 - 判断题
    private void CreateSubQuestionJudgmentUI(StackPanel container, Question subQuestion, string subQuestionId)
    {
        // 确保判断题有正确的选项
        if (subQuestion.Options == null || subQuestion.Options.Count < 2)
        {
            subQuestion.Options = new List<Option>
            {
                new Option("True", "True"),
                new Option("False", "False")
            };
        }
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        
        var trueOption = subQuestion.Options[0];
        var falseOption = subQuestion.Options[1];
        
        var trueRadio = new RadioButton
        {
            Content = "T. " + trueOption.Text,
            GroupName = $"Judgment_{subQuestionId}",
            Tag = trueOption.Id
        };
        
        var falseRadio = new RadioButton
        {
            Content = "F. " + falseOption.Text,
            GroupName = $"Judgment_{subQuestionId}",
            Tag = falseOption.Id
        };
        
        // 设置初始状态
        if (subQuestion.UserAnswer != null && subQuestion.UserAnswer.Length > 0)
        {
            string answer = subQuestion.UserAnswer[0];
            trueRadio.IsChecked = answer.Equals(trueOption.Id, StringComparison.OrdinalIgnoreCase);
            falseRadio.IsChecked = answer.Equals(falseOption.Id, StringComparison.OrdinalIgnoreCase);
        }
        
        // 设置事件处理
        trueRadio.Checked += (s, e) => 
        {
            if (trueRadio.Tag is string optionId)
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                subQuestion.UserAnswer = new[] { optionId };
                _viewModel.UpdateProgress();
            }
        };
        
        falseRadio.Checked += (s, e) => 
        {
            if (falseRadio.Tag is string optionId)
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                subQuestion.UserAnswer = new[] { optionId };
                _viewModel.UpdateProgress();
            }
        };
        
        panel.Children.Add(trueRadio);
        panel.Children.Add(falseRadio);
        container.Children.Add(panel);
        
        var radioButtons = new List<RadioButton> { trueRadio, falseRadio };
        _choiceAnswers[subQuestionId] = radioButtons;
    }

    // 子问题UI生成的辅助方法 - 文本题
    private void CreateSubQuestionTextAnswerUI(StackPanel container, Question subQuestion, string subQuestionId)
    {
        var textBox = new TextBox
        {
            Watermark = _localizationService["exam.answer.placeholder"],
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100
        };
        
        // 设置初始文本
        if (subQuestion.UserAnswer != null && subQuestion.UserAnswer.Length > 0)
        {
            textBox.Text = string.Join("\n", subQuestion.UserAnswer);
        }
        
        // 添加失去焦点处理
        textBox.LostFocus += (s, e) => 
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                subQuestion.UserAnswer = new[] { textBox.Text };
                _viewModel.UpdateProgress();
            }
        };
        
        _textAnswers[subQuestionId] = textBox;
        container.Children.Add(textBox);
    }

    
    // Helper method to get option letters
    private string[] GetOptionLetters(int count)
    {
        string[] letters = new string[count];
        for (int i = 0; i < count; i++)
        {
            // For 26 or fewer options, use A-Z
            if (i < 26)
            {
                letters[i] = ((char)('A' + i)).ToString();
            }
            else
            {
                // For more than 26 options, use AA, AB, etc.
                int first = (i / 26) - 1;
                int second = i % 26;
                letters[i] = $"{(char)('A' + first)}{(char)('A' + second)}";
            }
        }
        return letters;
    }
    
    private void CreateTextAnswerUI(string questionId)
    {
        var textBox = new TextBox
        {
            Watermark = _localizationService["exam.answer.placeholder"],
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 200
        };
        
        // Set initial text if available
        if (_viewModel.CurrentQuestion?.UserAnswer != null && _viewModel.CurrentQuestion.UserAnswer.Length > 0)
        {
            textBox.Text = string.Join("\n", _viewModel.CurrentQuestion.UserAnswer);
        }
        
        // Add lost focus handler to save answer
        textBox.LostFocus += (s, e) => 
        {
            if (_viewModel.CurrentQuestion != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                _configService.AppStatistics.AddQuestionInteractionCount(_configService);
                _viewModel.CurrentQuestion.UserAnswer = new[] { textBox.Text };
                _viewModel.UpdateProgress();
            }
        };
        
        // Store the text box for later retrieval
        _textAnswers[questionId] = textBox;
        
        AnswerContainer.Children.Add(textBox);
    }
    
    private void OnProgressUpdated(object? sender, EventArgs e)
    {
        // Update progress bar and percentage
        ProgressBar.Value = _viewModel.ProgressPercentage;
        ProgressBar.Maximum = 100;
        ProgressPercentText.Text = $"{_viewModel.ProgressPercentage:0}%";
        
        // Update time remaining text
        UpdateTimeDisplay();
        
        // Update the question list UI to show answered questions
        UpdateQuestionListUI();
    }
    
    private void UpdateQuestionListUI()
    {
        if (_viewModel.Examination == null) return;
        
        // For each section expander in the sections panel
        for (int sectionIndex = 0; sectionIndex < SectionsPanel.Children.Count; sectionIndex++)
        {
            if (SectionsPanel.Children[sectionIndex] is Expander expander &&
                expander.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is StackPanel questionsPanel &&
                sectionIndex < _viewModel.Examination.ExaminationSections.Length)
            {
                var section = _viewModel.Examination.ExaminationSections[sectionIndex];
                
                // Update each question button
                for (int questionIndex = 0; questionIndex < questionsPanel.Children.Count; questionIndex++)
                {
                    if (questionsPanel.Children[questionIndex] is Button button &&
                        section.Questions != null &&
                        questionIndex < section.Questions.Length)
                    {
                        var question = section.Questions[questionIndex];
                        bool hasAnswer = question.UserAnswer != null && question.UserAnswer.Length > 0;
                        
                        // Update button background
                        button.Background = hasAnswer 
                            ? new SolidColorBrush(Color.Parse("#22569AFF")) 
                            : new SolidColorBrush(Colors.Transparent);
                        
                        // Update check icon visibility
                        if (button.Content is Grid grid && grid.Children.Count >= 3)
                        {
                            grid.Children[2].IsVisible = hasAnswer;
                        }
                    }
                }
            }
        }
    }
    
    private async Task SubmitExamination()
    {
        if (_isSubmitting) return; // 防止重复提交
    
        try
        {
            _isSubmitting = true;
        
            // 禁用提交按钮防止重复点击
            if (SubmitButton != null)
            {
                SubmitButton.IsEnabled = false;
            }
        
            // Save current answers
            SaveCurrentAnswer();
        
            // Save accumulated time before submitting
            SaveAccumulatedTime();
        
            // Call ViewModel's submit method (非强制提交，会检查时间约束)
            await _viewModel.SubmitExamination(forceSubmit: false);
        
            // 如果成功提交，定时器会在ViewModel中停止
            // 如果由于时间约束失败，状态会在OnTimeConstraintViolated中重置
            
            // Show completion message
            // StatusMessageText.Text = "Examination submitted successfully!";
            // StatusOverlay.IsVisible = true;
            // StatusProgressBar.IsVisible = false;
            //
            // await Task.Delay(2000);
            // StatusOverlay.IsVisible = false;
            //
            // Navigate to results page or close window
            // Close();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error submitting examination: {ex.Message}");
            _logger.Trace($"Error submitting examination: {ex.StackTrace}");
            
            // Show error
            await ShowStatusMessage($"Error submitting examination: {ex.Message}", false);
        
            // 重置状态
            _isSubmitting = false;
            UpdateUIState();
        }
    }


    
    public void SaveCurrentAnswer()
    {
        if (_viewModel.CurrentQuestion == null) return;
        
        string questionId = _viewModel.CurrentQuestion.QuestionId ?? Guid.NewGuid().ToString();
        
        switch (_viewModel.CurrentQuestion.Type)
        {
            case QuestionTypes.Complex:
                // 复合题需要保存每个子问题的答案
                if (_viewModel.CurrentQuestion.SubQuestions != null)
                {
                    for (int i = 0; i < _viewModel.CurrentQuestion.SubQuestions.Count; i++)
                    {
                        var subQuestion = _viewModel.CurrentQuestion.SubQuestions[i];
                        string subQuestionId = subQuestion.QuestionId ?? $"{questionId}_sub_{i}";
                        
                        switch (subQuestion.Type)
                        {
                            case QuestionTypes.FillInTheBlank:
                            case QuestionTypes.ShortAnswer:
                            case QuestionTypes.Essay:
                            case QuestionTypes.Math:
                            case QuestionTypes.Calculation:
                            case QuestionTypes.Other:
                                if (_textAnswers.TryGetValue(subQuestionId, out var textBox) && !string.IsNullOrWhiteSpace(textBox.Text))
                                {
                                    subQuestion.UserAnswer = new[] { textBox.Text };
                                }
                                break;
                                
                            // 选择题通过UI事件直接处理
                        }
                    }
                }
                break;
                
            case QuestionTypes.FillInTheBlank:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Essay:
            case QuestionTypes.Math:
            case QuestionTypes.Calculation:
            case QuestionTypes.Other:
                if (_textAnswers.TryGetValue(questionId, out var theTextBox) && !string.IsNullOrWhiteSpace(theTextBox.Text))
                {
                    _viewModel.CurrentQuestion.UserAnswer = new[] { theTextBox.Text };
                }
                break;
        }
    }
    
    private void OnTimeConstraintViolated(object? sender, TimeConstraintEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                // 暂停定时器避免重复触发
                _uiUpdateTimer?.Stop();
            
                await ShowTimeConstraintDialog(e.Message);
            
                // 重置提交状态
                _isSubmitting = false;
            
                // 重新启动定时器
                RestartTimers();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling time constraint violation: {ex.Message}");
                _logger.Trace($"Error handling time constraint violation: {ex.StackTrace}");
                
                // 确保状态被重置
                _isSubmitting = false;
                RestartTimers();
                // 备用简单提示
                await ShowStatusMessage(e.Message, false);
            }
        });
    }
    
    private void RestartTimers()
    {
        try
        {
            // 确保定时器处于正确状态
            if (_viewModel?.Examination != null && 
                _configService?.AppData?.IsInExamination == true && 
                _viewModel.IsWindowVisible)
            {
                // 重启UI更新定时器
                if (_uiUpdateTimer != null && !_uiUpdateTimer.Enabled)
                {
                    _uiUpdateTimer.Start();
                }
            
                // 重启自动保存定时器
                if (_autoSaveTimer != null && !_autoSaveTimer.Enabled)
                {
                    _autoSaveTimer.Start();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error restarting timers: {ex.Message}");
            _logger.Trace($"Error restarting timers: {ex.StackTrace}");
        }
    }
    
    private void OnForceSubmitRequested(object? sender, EventArgs e)
    {
        if (_isSubmitting) return;
    
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await _viewModel.SubmitExamination(forceSubmit: true);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during force submit: {ex.Message}");
                _logger.Trace($"Error during force submit: {ex.StackTrace}");
                _isSubmitting = false;
            }
        });
    }

    
    protected override void OnClosed(EventArgs e)
    {
        try
        {
            // Save any pending changes
            SaveCurrentAnswer();
        
            // Save accumulated time before closing
            SaveAccumulatedTime();
        
            _ = _viewModel?.SaveProgressSilently();
        
            // Stop timers
            _uiUpdateTimer?.Stop();
            _autoSaveTimer?.Stop();
        
            // Unsubscribe from events
            if (_viewModel != null)
            {
                _viewModel.ExaminationLoaded -= OnExaminationLoaded;
                _viewModel.QuestionChanged -= OnQuestionChanged;
                _viewModel.ProgressUpdated -= OnProgressUpdated;
                _viewModel.TimeConstraintViolated -= OnTimeConstraintViolated;
                _viewModel.ForceSubmitRequested -= OnForceSubmitRequested;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during window close: {ex.Message}");
            _logger.Trace($"Error during window close: {ex.StackTrace}");
        }
        finally
        {
            base.OnClosed(e);
        }
    }
    
    private async Task ShowStatusMessage(string message, bool showProgress)
    {
        try
        {
            if (StatusMessageText != null)
                StatusMessageText.Text = message;
            if (StatusOverlay != null)
                StatusOverlay.IsVisible = true;
            if (StatusProgressBar != null)
                StatusProgressBar.IsVisible = showProgress;
        
            if (!showProgress)
            {
                await Task.Delay(3000);
                if (StatusOverlay != null)
                    StatusOverlay.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error showing status message: {ex.Message}");
            _logger.Trace($"Error showing status message: {ex.StackTrace}");
        }
    }
    
    private async Task ShowTimeConstraintDialog(string message)
    {
        var dialog = new Window
        {
            Title = _localizationService?["exam.submit.blocked.minimum.time"] ?? "Time Constraint",
            SizeToContent = SizeToContent.Manual,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = 500,
            Height = 200,
            MinWidth = 400,
            MinHeight = 150,
            MaxWidth = 700,
            MaxHeight = 300,
            CanResize = true,
            TransparencyLevelHint = new[] { Avalonia.Controls.WindowTransparencyLevel.AcrylicBlur },
            ExtendClientAreaToDecorationsHint = true
        };

        // Apply theme service for consistent styling
        var themeService = App.GetService<ThemeService>();
        themeService?.ApplyTransparencyToWindow(dialog);

        var grid = new Grid
        {
            Margin = new Thickness(20, 40, 20, 20),
            RowDefinitions = new RowDefinitions("*, Auto")
        };

        var messageTextBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            FontSize = 14
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 100,
            Height = 35,
            Classes = { "accent" },
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        // 对话框关闭处理
        var dialogClosed = false;
        okButton.Click += (s, e) => 
        {
            dialogClosed = true;
            dialog.Close();
        };

        // 允许直接关闭窗口
        dialog.Closing += (s, e) => 
        {
            dialogClosed = true;
        };

        buttonPanel.Children.Add(okButton);

        Grid.SetRow(messageTextBlock, 0);
        Grid.SetRow(buttonPanel, 1);

        grid.Children.Add(messageTextBlock);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;

        await dialog.ShowDialog(this);
        
        // 确保对话框关闭后状态正确
        if (dialogClosed)
        {
            // 触发UI更新以确保时间显示和按钮状态正确
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    UpdateTimeDisplay();
                    UpdateUIState();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error updating UI after dialog close: {ex.Message}");
                    _logger.Trace($"Error updating UI after dialog close: {ex.StackTrace}");
                }
            });
        }
    }
    
    private void UpdateUIState()
    {
        try
        {
            // 确保提交按钮可用（如果不是在提交过程中）
            if (SubmitButton != null && !_isSubmitting)
            {
                SubmitButton.IsEnabled = true;
            }
        
            // 确保其他按钮状态正确
            if (SaveButton != null && !_isSubmitting)
            {
                SaveButton.IsEnabled = true;
            }
        
            if (PrevButton != null && !_isSubmitting)
            {
                // 根据当前位置更新导航按钮状态
                PrevButton.IsEnabled = _viewModel?.CurrentSectionIndex > 0 || _viewModel?.CurrentQuestionIndex > 0;
            }
        
            if (NextButton != null && !_isSubmitting)
            {
                bool hasNextQuestion = false;
                if (_viewModel?.CurrentSection?.Questions != null)
                {
                    hasNextQuestion = _viewModel.CurrentQuestionIndex < _viewModel.CurrentSection.Questions.Length - 1;
                }
                bool hasNextSection = _viewModel?.CurrentSectionIndex < (_viewModel?.Examination?.ExaminationSections?.Length ?? 0) - 1;
                NextButton.IsEnabled = hasNextQuestion || hasNextSection;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating UI state: {ex.Message}");
            _logger.Trace($"Error updating UI state: {ex.StackTrace}");
        }
    }

}
