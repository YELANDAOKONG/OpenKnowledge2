using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DesktopKnowledgeAvalonia.Converters;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;
using LibraryOpenKnowledge.Models;

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
        
        // Apply theme
        _themeService.ApplyTransparencyToWindow(this);
        
        // Get the questions panel
        _questionsPanel = this.FindControl<StackPanel>("QuestionsPanel");
        
        // Subscribe to property change event
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Subscribe to window closing event
        this.Closing += OnWindowClosing;
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
        if (e.PropertyName == nameof(ExaminationResultWindowViewModel.QuestionScores))
        {
            GenerateQuestionControls();
        }
    }
    
    private void GenerateQuestionControls()
    {
        // Clear existing controls
        _questionsPanel.Children.Clear();
        
        // Loop through question scores and create UI for each
        int index = 0;
        foreach (var questionScore in _viewModel.QuestionScores)
        {
            index++;
            
            // Create container border
            var border = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#10FFFFFF")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
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
            
            // Section and type
            var sectionTypeStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };
            
            var sectionText = new TextBlock
            {
                Text = questionScore.SectionTitle,
                Opacity = 0.7,
                FontSize = 12
            };
            
            var dashText = new TextBlock
            {
                Text = "-",
                Opacity = 0.7,
                FontSize = 12
            };
            
            var typeText = new TextBlock
            {
                Text = (string)_questionTypeConverter.Convert(questionScore.QuestionType, typeof(string), null, null),
                Opacity = 0.7,
                FontSize = 12
            };
            
            sectionTypeStack.Children.Add(sectionText);
            sectionTypeStack.Children.Add(dashText);
            sectionTypeStack.Children.Add(typeText);
            
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
            
            // Add all to center stack
            centerStack.Children.Add(sectionTypeStack);
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
            
            // Right column: Result indicator
            var resultPanel = new Panel
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(10, 0, 0, 0)
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
            
            Grid.SetColumn(resultPanel, 2);
            grid.Children.Add(resultPanel);
            
            // Add grid to border
            border.Child = grid;
            
            // Add border to questions panel
            _questionsPanel.Children.Add(border);
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
    
    // Public method to update the sub-status text (for future customization)
    public void SetSubStatusText(string text)
    {
        if (_viewModel != null)
        {
            _viewModel.SubStatusText = text;
        }
    }
}
