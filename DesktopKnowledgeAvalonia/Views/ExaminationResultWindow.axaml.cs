﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
        
        // Get services
        _configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _themeService = App.GetService<ThemeService>();
        
        // Set up the view model
        _viewModel = new ExaminationResultWindowViewModel(_configService, _localizationService);
        DataContext = _viewModel;
        
        // Set flag to prevent main window from showing
        _viewModel.ShowMainWindow = false;
        
        // Apply theme
        _themeService.ApplyTransparencyToWindow(this);
        
        // Get the questions panel
        _questionsPanel = this.FindControl<StackPanel>("QuestionsPanel");
        
        // Subscribe to property change event
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Subscribe to window closing event
        this.Closing += OnWindowClosing;
        
        // Subscribe to the view model events
        _viewModel.SaveExaminationRequested += OnSaveExaminationRequested;
        _viewModel.ExitRequested += OnExitRequested;
    }
    
    // Constructor for immediate initialization with examination and score
    public ExaminationResultWindow(Examination examination, ScoreRecord scoreRecord) : this()
    {
        InitializeWithData(examination, scoreRecord);
    }
    
    // Alternative method to initialize after construction
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
        // Clear existing controls
        _questionsPanel.Children.Clear();
        
        // Process each section
        foreach (var section in _viewModel.SectionScores)
        {
            // Create section header
            var sectionHeader = new Expander
            {
                Header = section.SectionTitle,
                IsExpanded = true, // Default to expanded
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            // Create section content panel
            var sectionContent = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            // Loop through questions in this section
            foreach (var questionScore in section.Questions)
            {
                // Create container border for this question
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#10FFFFFF")),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                
                // Create grid with three columns
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
                
                var slashText = new TextBlock
                {
                    Text = "/"
                };
                
                var maxScoreText = new TextBlock
                {
                    Text = $"{questionScore.MaxScore:F1}"
                };
                
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
                    Spacing = 5
                };
                
                // Question type badge - FIX: Use Border + TextBlock instead of TextBlock with CornerRadius
                var typeBorder = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#15569AFF")),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(5, 2, 5, 2),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                
                var typeText = new TextBlock
                {
                    Text = (string)_questionTypeConverter.Convert(questionScore.QuestionType, typeof(string), null, null),
                    Opacity = 0.7,
                    FontSize = 12
                };
                
                typeBorder.Child = typeText;
                
                // AI Judged badge if applicable
                if (questionScore.IsAiJudged)
                {
                    // FIX: Use Border + TextBlock instead of TextBlock with CornerRadius
                    var aiJudgedBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#15FF8C00")),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(5, 2, 5, 2),
                        Margin = new Thickness(5, 0, 0, 5)
                    };
                    
                    var aiJudgedText = new TextBlock
                    {
                        Text = "AI Scored",
                        Opacity = 0.9,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.Parse("#FF8C00"))
                    };
                    
                    aiJudgedBorder.Child = aiJudgedText;
                    
                    // Create horizontal stack for badges
                    var badgesStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5
                    };
                    
                    badgesStack.Children.Add(typeBorder);
                    badgesStack.Children.Add(aiJudgedBorder);
                    
                    centerStack.Children.Add(badgesStack);
                }
                else
                {
                    centerStack.Children.Add(typeBorder);
                }
                
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
                
                // Add to center stack
                centerStack.Children.Add(stemText);
                centerStack.Children.Add(userAnswerStack);
                
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
                
                // Result indicator
                var resultPanel = new Panel
                {
                    Width = 32,
                    Height = 32,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                
                if (questionScore.IsCorrect)
                {
                    // Correct check icon
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
                    // Incorrect X icon
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
                        Content = "Rescore",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Padding = new Thickness(10, 5, 10, 5),
                        CommandParameter = questionScore.QuestionId
                    };
                    
                    // Bind the command
                    rescoreButton.Command = _viewModel.RescoreQuestionCommand;
                    
                    rightStack.Children.Add(rescoreButton);
                }
                
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(rightStack);
                
                // Add grid to border
                border.Child = grid;
                
                // Add border to section content
                sectionContent.Children.Add(border);
            }
            
            // Set section content
            sectionHeader.Content = sectionContent;
            
            // Add section to questions panel
            _questionsPanel.Children.Add(sectionHeader);
        }
    }
        
    private void OnWindowClosing(object sender, WindowClosingEventArgs e)
    {
        // Prevent closing if AI scoring is in progress (matches Exit button behavior)
        if (_viewModel.IsAiScoringInProgress)
        {
            e.Cancel = true;
        }
    }
    
    // Handle save examination request from ViewModel
    private async void OnSaveExaminationRequested(object sender, SaveExaminationEventArgs e)
    {
        if (e.Examination == null) return;
        
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
            
            // Show save dialog using this window's storage provider
            var result = await StorageProvider.SaveFilePickerAsync(options);
            
            if (result != null)
            {
                // Get file path
                var filePath = result.Path.LocalPath;
                
                // Save examination with user answers
                ExaminationSerializer.SerializeToFile(
                    e.Examination,
                    filePath,
                    includeUserAnswers: true);
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error saving examination: {ex.Message}");
            
            // Could show an error dialog here
        }
    }
    
    // Handle exit request
    private void OnExitRequested(object sender, EventArgs e)
    {
        // Ensure main window doesn't appear
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = false;
        }
        
        // Close the window
        Close();
    }
    
    // Public method to update the sub-status text (for future customization)
    public void SetSubStatusText(string text)
    {
        if (_viewModel != null)
        {
            _viewModel.SubStatusText = text;
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // Make sure main window doesn't appear after closing
        if (_configService.AppData != null)
        {
            _configService.AppData.IsInExamination = false;
        }
        
        base.OnClosed(e);
        
        // Unsubscribe from events to prevent memory leaks
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.SaveExaminationRequested -= OnSaveExaminationRequested;
            _viewModel.ExitRequested -= OnExitRequested;
        }
    }
}

