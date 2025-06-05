using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DesktopKnowledgeAvalonia.Converters;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;

namespace DesktopKnowledgeAvalonia.Views;

public partial class ExaminationResultWindow : AppWindowBase
{
    private readonly ExaminationResultWindowViewModel _viewModel;
    private readonly ConfigureService _configService;
    private readonly LocalizationService _localizationService;
    private readonly ThemeService _themeService;
    private readonly QuestionTypeConverter _questionTypeConverter = new QuestionTypeConverter();
    private readonly CorrectColorConverter _correctColorConverter = new CorrectColorConverter();
    private StackPanel _questionsPanel;
    
    public ExaminationResultWindow()
    {
        InitializeComponent();
        
        _configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _themeService = App.GetService<ThemeService>();
        
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = true;
            _configService.SaveChangesAsync();
        }
        
        _viewModel = new ExaminationResultWindowViewModel(_configService, _localizationService);
        DataContext = _viewModel;
        
        _viewModel.ShowMainWindow = false;
        _themeService.ApplyTransparencyToWindow(this);
        
        _questionsPanel = this.FindControl<StackPanel>("QuestionsPanel");
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        this.Closing += OnWindowClosing;
        _viewModel.SaveExaminationRequested += OnSaveExaminationRequested;
        _viewModel.ExitRequested += OnExitRequested;
    }
    
    public ExaminationResultWindow(Examination examination, ScoreRecord scoreRecord) : this()
    {
        InitializeWithData(examination, scoreRecord);
    }
    
    public async void InitializeWithData(Examination examination, ScoreRecord scoreRecord)
    {
        await _viewModel.InitializeAsync(examination, scoreRecord);
    }
    
    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExaminationResultWindowViewModel.SectionScores))
        {
            GenerateQuestionControls();
        }
    }
    
    private void GenerateQuestionControls()
    {
        _questionsPanel.Children.Clear();
        
        var masterExpander = new Expander
        {
            Header = _localizationService["exam.sections.questions"],
            IsExpanded = _viewModel.IsQuestionsPanelExpanded,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15)
        };
        
        masterExpander.Bind(Expander.IsExpandedProperty, new Avalonia.Data.Binding
        {
            Path = nameof(ExaminationResultWindowViewModel.IsQuestionsPanelExpanded),
            Mode = Avalonia.Data.BindingMode.TwoWay
        });
        
        var sectionsPanel = new StackPanel
        {
            Spacing = 15,
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        foreach (var section in _viewModel.SectionScores)
        {
            var sectionHeader = new Expander
            {
                Header = section.SectionTitle,
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            var sectionContent = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            foreach (var questionScore in section.Questions)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#10FFFFFF")),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                // Left column: Question number and score
                var leftStack = new StackPanel
                {
                    Margin = new Thickness(0, 0, 15, 0)
                };
                
                var questionNumberText = new TextBlock
                {
                    Text = $"#{questionScore.QuestionNumber}",
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 18
                };
                
                var scoreStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5
                };
                
                var obtainedScoreText = new TextBlock
                {
                    Text = $"{questionScore.ObtainedScore:F1}",
                    Foreground = (IBrush)_correctColorConverter.Convert(questionScore.IsCorrect, typeof(IBrush), null, null)
                };
                
                var slashText = new TextBlock { Text = "/" };
                var maxScoreText = new TextBlock { Text = $"{questionScore.MaxScore:F1}" };
                
                scoreStack.Children.Add(obtainedScoreText);
                scoreStack.Children.Add(slashText);
                scoreStack.Children.Add(maxScoreText);
                
                leftStack.Children.Add(questionNumberText);
                leftStack.Children.Add(scoreStack);
                
                Grid.SetColumn(leftStack, 0);
                grid.Children.Add(leftStack);
                
                // Center column: Question details
                var centerStack = new StackPanel
                {
                    Spacing = 5,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                
                // Question type and AI judged badges
                var badgesStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5
                };
                
                var typeBorder = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#15569AFF")),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(5, 2, 5, 2)
                };
                
                var typeText = new TextBlock
                {
                    Text = (string)_questionTypeConverter.Convert(questionScore.QuestionType, typeof(string), null, null),
                    Opacity = 0.7,
                    FontSize = 12
                };
                
                typeBorder.Child = typeText;
                badgesStack.Children.Add(typeBorder);
                
                if (questionScore.IsAiJudged)
                {
                    var aiJudgedBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#15FF8C00")),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    
                    var aiJudgedText = new TextBlock
                    {
                        Text = _localizationService["exam.result.ai.scored"],
                        Opacity = 0.9,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.Parse("#FF8C00"))
                    };
                    
                    aiJudgedBorder.Child = aiJudgedText;
                    badgesStack.Children.Add(aiJudgedBorder);
                    
                    // Add unevaluated badge if not evaluated
                    if (!questionScore.IsEvaluated)
                    {
                        var unevaluatedBorder = new Border
                        {
                            Background = new SolidColorBrush(Color.Parse("#15FFA500")),
                            CornerRadius = new CornerRadius(3),
                            Padding = new Thickness(5, 2, 5, 2)
                        };
                        
                        var unevaluatedText = new TextBlock
                        {
                            Text = _localizationService["exam.result.unevaluated"],
                            Opacity = 0.9,
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#FFA500"))
                        };
                        
                        unevaluatedBorder.Child = unevaluatedText;
                        badgesStack.Children.Add(unevaluatedBorder);
                    }
                }
                
                centerStack.Children.Add(badgesStack);
                
                // Question stem
                var stemText = new TextBlock
                {
                    Text = questionScore.QuestionStem,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                
                // User answer
                var userAnswerStack = new StackPanel();
                var userAnswerLabel = new TextBlock
                {
                    Text = _localizationService["exam.result.your.answer"],
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 12,
                    Margin = new Thickness(0, 5, 0, 2)
                };
                
                var userAnswerText = new TextBlock
                {
                    Text = questionScore.UserAnswer,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.9
                };
                
                userAnswerStack.Children.Add(userAnswerLabel);
                userAnswerStack.Children.Add(userAnswerText);
                
                centerStack.Children.Add(stemText);
                centerStack.Children.Add(userAnswerStack);
                
                // AI Feedback (if available)
                if (questionScore.IsAiJudged && !string.IsNullOrEmpty(questionScore.AiFeedback))
                {
                    var aiFeedbackStack = new StackPanel();
                    var aiFeedbackLabel = new TextBlock
                    {
                        Text = _localizationService["exam.result.ai.feedback"],
                        FontWeight = FontWeight.SemiBold,
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 2)
                    };
                    
                    var aiFeedbackText = new TextBlock
                    {
                        Text = questionScore.AiFeedback,
                        TextWrapping = TextWrapping.Wrap,
                        Opacity = 0.9
                    };
                    
                    aiFeedbackStack.Children.Add(aiFeedbackLabel);
                    aiFeedbackStack.Children.Add(aiFeedbackText);
                    centerStack.Children.Add(aiFeedbackStack);
                }
                
                // Correct answer (if available)
                if (!string.IsNullOrEmpty(questionScore.CorrectAnswer))
                {
                    var correctAnswerStack = new StackPanel();
                    var correctAnswerLabel = new TextBlock
                    {
                        Text = _localizationService["exam.result.correct.answer"],
                        FontWeight = FontWeight.SemiBold,
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 2)
                    };
                    
                    var correctAnswerText = new TextBlock
                    {
                        Text = questionScore.CorrectAnswer,
                        TextWrapping = TextWrapping.Wrap,
                        Opacity = 0.9
                    };
                    
                    correctAnswerStack.Children.Add(correctAnswerLabel);
                    correctAnswerStack.Children.Add(correctAnswerText);
                    centerStack.Children.Add(correctAnswerStack);
                }
                
                Grid.SetColumn(centerStack, 1);
                grid.Children.Add(centerStack);
                
                // Right column: Result indicator and Rescore button
                var rightStack = new StackPanel
                {
                    Spacing = 10,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                
                var resultPanel = new Panel
                {
                    Width = 32,
                    Height = 32,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                
                // Show appropriate icon based on evaluation state
                if (questionScore.IsAiJudged && !questionScore.IsEvaluated)
                {
                    // Question mark icon for unevaluated AI questions
                    var questionMarkIcon = new PathIcon
                    {
                        Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z"),
                        Width = 24,
                        Height = 24,
                        Foreground = new SolidColorBrush(Color.Parse("#FFA500"))
                    };
                    resultPanel.Children.Add(questionMarkIcon);
                }
                else if (questionScore.IsCorrect)
                {
                    var checkIcon = new PathIcon
                    {
                        Data = Geometry.Parse("M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"),
                        Width = 24,
                        Height = 24,
                        Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
                    };
                    resultPanel.Children.Add(checkIcon);
                }
                else
                {
                    var xIcon = new PathIcon
                    {
                        Data = Geometry.Parse("M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"),
                        Width = 24,
                        Height = 24,
                        Foreground = new SolidColorBrush(Color.Parse("#F44336"))
                    };
                    resultPanel.Children.Add(xIcon);
                }
                
                rightStack.Children.Add(resultPanel);
                
                // Add rescore button for AI-judged questions (only available after initial scoring)
                if (questionScore.IsAiJudged)
                {
                    var rescoreButton = new Button
                    {
                        Content = _localizationService["exam.result.rescore"],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Padding = new Thickness(10, 5, 10, 5),
                        CommandParameter = questionScore.QuestionId
                    };
                    
                    // Bind the command and enable state
                    rescoreButton.Command = _viewModel.RescoreQuestionCommand;
                    rescoreButton.Bind(Button.IsEnabledProperty, new Avalonia.Data.Binding
                    {
                        Path = nameof(ExaminationResultWindowViewModel.HasPerformedInitialAiScoring),
                        Mode = Avalonia.Data.BindingMode.OneWay
                    });
                    
                    rightStack.Children.Add(rescoreButton);
                }
                
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(rightStack);
                
                border.Child = grid;
                sectionContent.Children.Add(border);
            }
            
            sectionHeader.Content = sectionContent;
            sectionsPanel.Children.Add(sectionHeader);
        }
        
        masterExpander.Content = sectionsPanel;
        _questionsPanel.Children.Add(masterExpander);
    }
    
    private void OnWindowClosing(object sender, WindowClosingEventArgs e)
    {
        if (_viewModel.IsAiScoringInProgress)
        {
            e.Cancel = true;
        }
        else
        {
            if (_configService.AppData != null)
            {
                _configService.AppData.IsInExamination = false;
                _configService.SaveChangesAsync();
            }
        }
    }
    
    private async void OnSaveExaminationRequested(object sender, SaveExaminationEventArgs e)
    {
        if (e.Examination == null) return;
        
        try
        {
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
            
            var result = await StorageProvider.SaveFilePickerAsync(options);
            
            if (result != null)
            {
                var filePath = result.Path.LocalPath;
                ExaminationSerializer.SerializeToFile(e.Examination, filePath, includeUserAnswers: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving examination: {ex.Message}");
        }
    }
    
    private void OnExitRequested(object sender, EventArgs e)
    {
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = false;
            _configService.SaveChangesAsync();
        }
    
        Close();
    
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            });
        });
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = true;
            _configService.SaveChangesAsync();
        }
    }
    
    public void SetSubStatusText(string text)
    {
        if (_viewModel != null)
        {
            _viewModel.SubStatusText = text;
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = false;
            _configService.SaveChangesAsync();
        }
        
        base.OnClosed(e);
        
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.SaveExaminationRequested -= OnSaveExaminationRequested;
            _viewModel.ExitRequested -= OnExitRequested;
        }
    }
}
