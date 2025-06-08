using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
        // 完全清空并重新构建
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
                // AI Feedback (if available and evaluated)
                if (questionScore.IsAiJudged && questionScore.IsEvaluated && !string.IsNullOrEmpty(questionScore.AiFeedback))
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
                // Add rescore button for AI-judged questions
                if (questionScore.IsAiJudged)
                {
                    var rescoreButton = new Button
                    {
                        Content = _localizationService["exam.result.rescore"],
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    rescoreButton.Command = _viewModel.RescoreQuestionCommand;
                    rescoreButton.CommandParameter = questionScore.QuestionId;
    
                    // 控制按钮启用状态：必须有答案且已进行过初始评分
                    bool hasAnswer = !questionScore.UserAnswer.Equals(_localizationService["exam.result.no.answer"]);
    
                    // 基础状态设置 - 只有当有答案且完成初始AI评分时才启用
                    var binding = new MultiBinding
                    {
                        Converter = new BooleanMultiConverter()
                    };
    
                    binding.Bindings.Add(new Binding("HasPerformedInitialAiScoring"));
                    binding.Bindings.Add(new Binding("!IsAiScoringInProgress"));
    
                    // 如果没有答案，直接禁用
                    rescoreButton.IsEnabled = hasAnswer; 
    
                    // 只有当有答案时才应用其他条件绑定
                    if (hasAnswer)
                    {
                        rescoreButton.Bind(Button.IsEnabledProperty, binding);
                    }
                    rightStack.Children.Add(rescoreButton);
                }
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(rightStack);
                border.Child = grid;
                sectionContent.Children.Add(border);
                
                
                // 如果是包含子问题的复合题，添加子问题
                if (questionScore.QuestionType == QuestionTypes.Complex && questionScore.SubQuestions != null && questionScore.SubQuestions.Count > 0)
                {
                    // 创建子问题容器
                    var subQuestionsPanel = new StackPanel
                    {
                        Margin = new Thickness(20, 5, 0, 0),
                        Spacing = 5
                    };
                    
                    // 为每个子问题创建UI
                    for (int i = 0; i < questionScore.SubQuestions.Count; i++)
                    {
                        var subQuestion = questionScore.SubQuestions[i];
                        
                        // 创建子问题边框
                        var subBorder = new Border
                        {
                            Background = new SolidColorBrush(Color.Parse("#15FFFFFF")),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(10),
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        
                        // 创建子问题网格
                        var subGrid = new Grid();
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        
                        // 左侧列：子问题编号和分数
                        var subLeftStack = new StackPanel
                        {
                            Margin = new Thickness(0, 0, 15, 0)
                        };
                        
                        var subNumberText = new TextBlock
                        {
                            Text = $"#{questionScore.QuestionNumber}.{i + 1}",
                            FontWeight = FontWeight.Normal,
                            FontSize = 14
                        };
                        
                        var subScoreStack = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5
                        };
                        
                        var subObtainedScoreText = new TextBlock
                        {
                            Text = $"{subQuestion.ObtainedScore:F1}",
                            Foreground = (IBrush)_correctColorConverter.Convert(subQuestion.IsCorrect, typeof(IBrush), null, null)
                        };
                        
                        var subSlashText = new TextBlock { Text = "/" };
                        var subMaxScoreText = new TextBlock { Text = $"{subQuestion.MaxScore:F1}" };
                        
                        subScoreStack.Children.Add(subObtainedScoreText);
                        subScoreStack.Children.Add(subSlashText);
                        subScoreStack.Children.Add(subMaxScoreText);
                        
                        subLeftStack.Children.Add(subNumberText);
                        subLeftStack.Children.Add(subScoreStack);
                        
                        Grid.SetColumn(subLeftStack, 0);
                        subGrid.Children.Add(subLeftStack);
                        
                        // 中间列：子问题详情
                        var subCenterStack = new StackPanel
                        {
                            Spacing = 5,
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };
                        
                        // 问题类型标签
                        var subBadgesStack = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5
                        };
                        
                        var subTypeBorder = new Border
                        {
                            Background = new SolidColorBrush(Color.Parse("#15569AFF")),
                            CornerRadius = new CornerRadius(3),
                            Padding = new Thickness(5, 2, 5, 2)
                        };
                        
                        var subTypeText = new TextBlock
                        {
                            Text = (string)_questionTypeConverter.Convert(subQuestion.QuestionType, typeof(string), null, null),
                            Opacity = 0.7,
                            FontSize = 12
                        };
                        
                        subTypeBorder.Child = subTypeText;
                        subBadgesStack.Children.Add(subTypeBorder);
                        
                        // 如果适用，添加AI评分标签
                        if (subQuestion.IsAiJudged)
                        {
                            var subAiJudgedBorder = new Border
                            {
                                Background = new SolidColorBrush(Color.Parse("#15FF8C00")),
                                CornerRadius = new CornerRadius(3),
                                Padding = new Thickness(5, 2, 5, 2)
                            };
                            
                            var subAiJudgedText = new TextBlock
                            {
                                Text = _localizationService["exam.result.ai.scored"],
                                Opacity = 0.9,
                                FontSize = 12,
                                Foreground = new SolidColorBrush(Color.Parse("#FF8C00"))
                            };
                            
                            subAiJudgedBorder.Child = subAiJudgedText;
                            subBadgesStack.Children.Add(subAiJudgedBorder);
                            
                            // 如果未评估，添加未评估标签
                            if (!subQuestion.IsEvaluated)
                            {
                                var subUnevaluatedBorder = new Border
                                {
                                    Background = new SolidColorBrush(Color.Parse("#15FFA500")),
                                    CornerRadius = new CornerRadius(3),
                                    Padding = new Thickness(5, 2, 5, 2)
                                };
                                
                                var subUnevaluatedText = new TextBlock
                                {
                                    Text = _localizationService["exam.result.unevaluated"],
                                    Opacity = 0.9,
                                    FontSize = 12,
                                    Foreground = new SolidColorBrush(Color.Parse("#FFA500"))
                                };
                                
                                subUnevaluatedBorder.Child = subUnevaluatedText;
                                subBadgesStack.Children.Add(subUnevaluatedBorder);
                            }
                        }
                        
                        subCenterStack.Children.Add(subBadgesStack);
                        
                        // 子问题题干
                        var subStemText = new TextBlock
                        {
                            Text = subQuestion.QuestionStem,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        
                        // 用户答案
                        var subUserAnswerStack = new StackPanel();
                        var subUserAnswerLabel = new TextBlock
                        {
                            Text = _localizationService["exam.result.your.answer"],
                            FontWeight = FontWeight.SemiBold,
                            FontSize = 12,
                            Margin = new Thickness(0, 5, 0, 2)
                        };
                        
                        var subUserAnswerText = new TextBlock
                        {
                            Text = subQuestion.UserAnswer,
                            TextWrapping = TextWrapping.Wrap,
                            Opacity = 0.9
                        };
                        
                        subUserAnswerStack.Children.Add(subUserAnswerLabel);
                        subUserAnswerStack.Children.Add(subUserAnswerText);
                        
                        subCenterStack.Children.Add(subStemText);
                        subCenterStack.Children.Add(subUserAnswerStack);
                        
                        // AI反馈（如果有且已评估）
                        if (subQuestion.IsAiJudged && subQuestion.IsEvaluated && !string.IsNullOrEmpty(subQuestion.AiFeedback))
                        {
                            var subAiFeedbackStack = new StackPanel();
                            var subAiFeedbackLabel = new TextBlock
                            {
                                Text = _localizationService["exam.result.ai.feedback"],
                                FontWeight = FontWeight.SemiBold,
                                FontSize = 12,
                                Margin = new Thickness(0, 5, 0, 2)
                            };
                            
                            var subAiFeedbackText = new TextBlock
                            {
                                Text = subQuestion.AiFeedback,
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.9
                            };
                            
                            subAiFeedbackStack.Children.Add(subAiFeedbackLabel);
                            subAiFeedbackStack.Children.Add(subAiFeedbackText);
                            
                            subCenterStack.Children.Add(subAiFeedbackStack);
                        }
                        
                        // 正确答案（如果有）
                        if (!string.IsNullOrEmpty(subQuestion.CorrectAnswer))
                        {
                            var subCorrectAnswerStack = new StackPanel();
                            var subCorrectAnswerLabel = new TextBlock
                            {
                                Text = _localizationService["exam.result.correct.answer"],
                                FontWeight = FontWeight.SemiBold,
                                FontSize = 12,
                                Margin = new Thickness(0, 5, 0, 2)
                            };
                            
                            var subCorrectAnswerText = new TextBlock
                            {
                                Text = subQuestion.CorrectAnswer,
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.9
                            };
                            
                            subCorrectAnswerStack.Children.Add(subCorrectAnswerLabel);
                            subCorrectAnswerStack.Children.Add(subCorrectAnswerText);
                            
                            subCenterStack.Children.Add(subCorrectAnswerStack);
                        }
                        
                        Grid.SetColumn(subCenterStack, 1);
                        subGrid.Children.Add(subCenterStack);
                        
                        // 右侧列：结果指示
                        var subRightStack = new StackPanel
                        {
                            Spacing = 10,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        
                        var subResultPanel = new Panel
                        {
                            Width = 24,
                            Height = 24,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        
                        // 根据评估状态显示适当的图标
                        if (subQuestion.IsAiJudged && !subQuestion.IsEvaluated)
                        {
                            // 未评估AI问题的问号图标
                            var subQuestionMarkIcon = new PathIcon
                            {
                                Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z"),
                                Width = 20,
                                Height = 20,
                                Foreground = new SolidColorBrush(Color.Parse("#FFA500"))
                            };
                            subResultPanel.Children.Add(subQuestionMarkIcon);
                        }
                        else if (subQuestion.IsCorrect)
                        {
                            var subCheckIcon = new PathIcon
                            {
                                Data = Geometry.Parse("M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"),
                                Width = 20,
                                Height = 20,
                                Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
                            };
                            subResultPanel.Children.Add(subCheckIcon);
                        }
                        else
                        {
                            var subXIcon = new PathIcon
                            {
                                Data = Geometry.Parse("M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"),
                                Width = 20,
                                Height = 20,
                                Foreground = new SolidColorBrush(Color.Parse("#F44336"))
                            };
                            subResultPanel.Children.Add(subXIcon);
                        }
                        
                        subRightStack.Children.Add(subResultPanel);
                        
                        // 为AI评分题添加重评按钮
                        if (subQuestion.IsAiJudged)
                        {
                            var subRescoreButton = new Button
                            {
                                Content = _localizationService["exam.result.rescore"],
                                HorizontalAlignment = HorizontalAlignment.Right
                            };
                            
                            subRescoreButton.Command = _viewModel.RescoreQuestionCommand;
                            subRescoreButton.CommandParameter = subQuestion.QuestionId;
                            
                            // 控制按钮启用状态
                            bool hasAnswer = !subQuestion.UserAnswer.Equals(_localizationService["exam.result.no.answer"]);
                            
                            // 基础状态 - 只有当有答案且初始AI评分完成时才启用
                            var binding = new MultiBinding
                            {
                                Converter = new BooleanMultiConverter()
                            };
                            
                            binding.Bindings.Add(new Binding("HasPerformedInitialAiScoring"));
                            binding.Bindings.Add(new Binding("!IsAiScoringInProgress"));
                            
                            // 如果没有答案，直接禁用
                            subRescoreButton.IsEnabled = hasAnswer;
                            
                            // 只有当有答案时才应用其他条件绑定
                            if (hasAnswer)
                            {
                                subRescoreButton.Bind(Button.IsEnabledProperty, binding);
                            }
                            
                            subRightStack.Children.Add(subRescoreButton);
                        }
                        
                        Grid.SetColumn(subRightStack, 2);
                        subGrid.Children.Add(subRightStack);
                        
                        subBorder.Child = subGrid;
                        subQuestionsPanel.Children.Add(subBorder);
                    }
                    
                    centerStack.Children.Add(subQuestionsPanel);
                }
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
    
        // Task.Delay(100).ContinueWith(_ =>
        // {
        //     Dispatcher.UIThread.InvokeAsync(() =>
        //     {
        //         var mainWindow = new MainWindow();
        //         mainWindow.Show();
        //     });
        // });
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
