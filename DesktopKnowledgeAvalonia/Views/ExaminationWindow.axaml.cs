using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;
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
    
    // Constructor with optional parameters for loading examinations
    public ExaminationWindow(string? filePath = null, bool force = false)
    {
        InitializeComponent();
        
        // Get services
        _localizationService = App.GetService<LocalizationService>();
        _configService = App.GetService<ConfigureService>();
        _viewModel = new ExaminationWindowViewModel(_configService, _localizationService);
        
        // Set DataContext for convenience
        DataContext = _viewModel;
        
        // Subscribe to view model events
        _viewModel.ExaminationLoaded += OnExaminationLoaded;
        _viewModel.QuestionChanged += OnQuestionChanged;
        _viewModel.ProgressUpdated += OnProgressUpdated;
        
        // Initialize UI text
        InitializeUI();
        
        // Set up event handlers
        SetupEventHandlers();
        
        // Load examination from file or current examination
        LoadExamination(filePath, force);
        
        // Initialize timers
        InitializeTimers();
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
        BackButton.Click += (s, e) => _viewModel.BackToMain();
        SaveButton.Click += async (s, e) => await _viewModel.SaveProgress();
        SubmitButton.Click += async (s, e) => await SubmitExamination();
        PrevButton.Click += (s, e) => _viewModel.NavigatePrevious();
        NextButton.Click += (s, e) => _viewModel.NavigateNext();
    }
    
    private void InitializeTimers()
    {
        // UI update timer (0.5 seconds)
        _uiUpdateTimer = new System.Timers.Timer(500);
        _uiUpdateTimer.Elapsed += (s, e) => 
        {
            Dispatcher.UIThread.InvokeAsync(() => 
            {
                if (_viewModel.Examination != null)
                {
                    UpdateTimeDisplay();
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
                if (_viewModel.Examination != null && _configService.AppData.IsInExamination)
                {
                    // First save current answer
                    SaveCurrentAnswer();
                    // Then silently save to disk
                    await _viewModel.SaveProgressSilently();
                }
            });
        };
        _autoSaveTimer.Start();
    }
    
    private void UpdateTimeDisplay()
    {
        if (_configService.AppData.ExaminationTimer.HasValue)
        {
            var elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _configService.AppData.ExaminationTimer.Value;
            var timeSpan = TimeSpan.FromMilliseconds(elapsed);
            string timeFormat = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            _viewModel.TimeRemaining = timeFormat;
            
            TimeRemainingText.Text = string.Format(_localizationService["exam.time.remaining"], timeFormat);
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
                    _configService.AppData.ExaminationTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    _configService.SaveChangesAsync();
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
                    
                    // Add question title
                    var titleText = new TextBlock
                    {
                        Text = question.Stem,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    Grid.SetColumn(titleText, 1);
                    grid.Children.Add(titleText);
                    
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
    
    private void OnQuestionChanged(object? sender, EventArgs e)
    {
        if (_viewModel.CurrentQuestion == null) return;
        
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
        
        // Show/hide reference materials
        bool hasReferenceMaterials = _viewModel.CurrentQuestion.ReferenceMaterials != null && 
                                    _viewModel.CurrentQuestion.ReferenceMaterials.Length > 0;
        
        ReferenceMaterialsExpander.IsVisible = hasReferenceMaterials;
        ReferenceMaterialsExpander.Header = _localizationService["exam.reference"];
        
        if (hasReferenceMaterials)
        {
            BuildReferenceMaterialsUI();
        }
        
        // Build answer UI based on question type
        BuildAnswerUI();
        
        // Update section expandable state
        for (int i = 0; i < SectionsPanel.Children.Count; i++)
        {
            if (SectionsPanel.Children[i] is Expander expander)
            {
                expander.IsExpanded = i == _viewModel.CurrentSectionIndex;
            }
        }
    }
    
    private void BuildReferenceMaterialsUI()
    {
        ReferenceMaterialsPanel.Children.Clear();
        
        if (_viewModel.CurrentQuestion?.ReferenceMaterials == null) return;
        
        foreach (var material in _viewModel.CurrentQuestion.ReferenceMaterials)
        {
            // Add text materials
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
                    ReferenceMaterialsPanel.Children.Add(textBlock);
                }
            }
            
            // Add images
            if (material.Images != null && material.Images.Length > 0)
            {
                foreach (var img in material.Images)
                {
                    try
                    {
                        Image image = null;
                        
                        switch (img.Type)
                        {
                            case ReferenceMaterialImageTypes.Local:
                            case ReferenceMaterialImageTypes.Remote:
                                if (!string.IsNullOrEmpty(img.Uri))
                                {
                                    image = new Image
                                    {
                                        Source = new Bitmap(img.Uri),
                                        MaxHeight = 300,
                                        Margin = new Thickness(0, 10)
                                    };
                                }
                                break;
                                
                            case ReferenceMaterialImageTypes.Embedded:
                                if (img.Image != null)
                                {
                                    using var stream = new System.IO.MemoryStream(img.Image);
                                    image = new Image
                                    {
                                        Source = new Bitmap(stream),
                                        MaxHeight = 300,
                                        Margin = new Thickness(0, 10)
                                    };
                                }
                                break;
                        }
                        
                        if (image != null)
                        {
                            ReferenceMaterialsPanel.Children.Add(image);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Add error text instead
                        var errorText = new TextBlock
                        {
                            Text = $"Error loading image: {ex.Message}",
                            Foreground = new SolidColorBrush(Colors.Red),
                            Margin = new Thickness(0, 5)
                        };
                        ReferenceMaterialsPanel.Children.Add(errorText);
                    }
                }
            }
        }
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
                
            case QuestionTypes.FillInTheBlank:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Essay:
            case QuestionTypes.Math:
            case QuestionTypes.Calculation:
            case QuestionTypes.Complex:
            case QuestionTypes.Other:
                CreateTextAnswerUI(questionId);
                break;
        }
    }
    
    private void CreateSingleChoiceUI(string questionId)
    {
        if (_viewModel.CurrentQuestion?.Options == null) return;
        
        var radioButtons = new List<RadioButton>();
        
        foreach (var option in _viewModel.CurrentQuestion.Options)
        {
            var radioButton = new RadioButton
            {
                Content = option.Item2,
                GroupName = $"SingleChoice_{questionId}",
                Margin = new Thickness(0, 5)
            };
            
            // Check if this option is selected
            if (_viewModel.CurrentQuestion.UserAnswer != null && 
                _viewModel.CurrentQuestion.UserAnswer.Contains(option.Item1))
            {
                radioButton.IsChecked = true;
            }
            
            // Set handler
            string optionId = option.Item1;
            radioButton.Checked += (s, e) => 
            {
                if (_viewModel.CurrentQuestion != null)
                {
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
        if (_viewModel.CurrentQuestion?.Options == null) return;
        
        var checkBoxes = new List<CheckBox>();
        
        foreach (var option in _viewModel.CurrentQuestion.Options)
        {
            var checkBox = new CheckBox
            {
                Content = option.Item2,
                Margin = new Thickness(0, 5)
            };
            
            // Check if this option is selected
            if (_viewModel.CurrentQuestion.UserAnswer != null && 
                _viewModel.CurrentQuestion.UserAnswer.Contains(option.Item1))
            {
                checkBox.IsChecked = true;
            }
            
            // Set handler
            string optionId = option.Item1;
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
        
        var checkBoxes = _choiceAnswers[questionId] as List<CheckBox>;
        if (checkBoxes == null) return;
        
        var selectedOptions = new List<string>();
        
        for (int i = 0; i < checkBoxes.Count; i++)
        {
            if (checkBoxes[i].IsChecked == true && 
                _viewModel.CurrentQuestion.Options != null && 
                i < _viewModel.CurrentQuestion.Options.Count)
            {
                selectedOptions.Add(_viewModel.CurrentQuestion.Options[i].Item1);
            }
        }
        
        _viewModel.CurrentQuestion.UserAnswer = selectedOptions.ToArray();
        _viewModel.UpdateProgress();
    }
    
    private void CreateJudgmentUI(string questionId)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        
        var trueRadio = new RadioButton
        {
            Content = "True",
            GroupName = $"Judgment_{questionId}"
        };
        
        var falseRadio = new RadioButton
        {
            Content = "False",
            GroupName = $"Judgment_{questionId}"
        };
        
        // Set initial state
        if (_viewModel.CurrentQuestion?.UserAnswer != null && _viewModel.CurrentQuestion.UserAnswer.Length > 0)
        {
            string answer = _viewModel.CurrentQuestion.UserAnswer[0];
            trueRadio.IsChecked = answer.Equals("True", StringComparison.OrdinalIgnoreCase);
            falseRadio.IsChecked = answer.Equals("False", StringComparison.OrdinalIgnoreCase);
        }
        
        // Set handlers
        trueRadio.Checked += (s, e) => 
        {
            if (_viewModel.CurrentQuestion != null)
            {
                _viewModel.CurrentQuestion.UserAnswer = new[] { "True" };
                _viewModel.UpdateProgress();
            }
        };
        
        falseRadio.Checked += (s, e) => 
        {
            if (_viewModel.CurrentQuestion != null)
            {
                _viewModel.CurrentQuestion.UserAnswer = new[] { "False" };
                _viewModel.UpdateProgress();
            }
        };
        
        panel.Children.Add(trueRadio);
        panel.Children.Add(falseRadio);
        
        AnswerContainer.Children.Add(panel);
        
        var radioButtons = new List<RadioButton> { trueRadio, falseRadio };
        _choiceAnswers[questionId] = radioButtons;
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
        try
        {
            // Save current answers
            SaveCurrentAnswer();
            
            // Call ViewModel's submit method
            await _viewModel.SubmitExamination();
            
            // Stop timers
            _uiUpdateTimer.Stop();
            _autoSaveTimer.Stop();
            
            // Show completion message
            StatusMessageText.Text = "Examination submitted successfully!";
            StatusOverlay.IsVisible = true;
            StatusProgressBar.IsVisible = false;
            
            await Task.Delay(2000);
            StatusOverlay.IsVisible = false;
            
            // TODO: Navigate to results page or close window
            Close();
        }
        catch (Exception ex)
        {
            // Show error
            StatusMessageText.Text = $"Error submitting examination: {ex.Message}";
            StatusOverlay.IsVisible = true;
            StatusProgressBar.IsVisible = false;
            
            await Task.Delay(3000);
            StatusOverlay.IsVisible = false;
        }
    }
    
    public void SaveCurrentAnswer()
    {
        if (_viewModel.CurrentQuestion == null) return;
        
        string questionId = _viewModel.CurrentQuestion.QuestionId ?? Guid.NewGuid().ToString();
        
        // Save answer based on question type
        switch (_viewModel.CurrentQuestion.Type)
        {
            case QuestionTypes.FillInTheBlank:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Essay:
            case QuestionTypes.Math:
            case QuestionTypes.Calculation:
            case QuestionTypes.Complex:
            case QuestionTypes.Other:
                if (_textAnswers.TryGetValue(questionId, out var textBox) && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    _viewModel.CurrentQuestion.UserAnswer = new[] { textBox.Text };
                }
                break;
                
            // Other question types are handled through direct UI element events
        }
        
        // Assign the implementation to the ViewModel's method
        _viewModel.SaveCurrentAnswer = SaveCurrentAnswer;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // Save any pending changes
        SaveCurrentAnswer();
        _viewModel.SaveProgressSilently();
        
        // Stop timers
        _uiUpdateTimer.Stop();
        _autoSaveTimer.Stop();
        
        // Unsubscribe from events
        _viewModel.ExaminationLoaded -= OnExaminationLoaded;
        _viewModel.QuestionChanged -= OnQuestionChanged;
        _viewModel.ProgressUpdated -= OnProgressUpdated;
    }
}
