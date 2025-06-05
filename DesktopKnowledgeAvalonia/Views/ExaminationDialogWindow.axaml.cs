using Avalonia.Layout;

namespace DesktopKnowledgeAvalonia.Views;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DesktopKnowledgeAvalonia.ViewModels;
using DesktopKnowledgeAvalonia.Services;

public partial class ExaminationDialogWindow : AppWindowBase
{
    private readonly ExaminationDialogWindowViewModel _windowViewModel;
    private readonly LocalizationService _localizationService;
    
    public ExaminationDialogWindow()
    {
        InitializeComponent();
        
        // Create and set the view model
        var configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        
        _windowViewModel = new ExaminationDialogWindowViewModel(configService, _localizationService);
        DataContext = _windowViewModel;
        
        // Subscribe to events
        _windowViewModel.CloseRequested += (s, e) => Close();
        _windowViewModel.ContinueExamRequested += OnContinueExam;
        _windowViewModel.LoadNewExamRequested += OnLoadNewExam;
        _windowViewModel.DeleteCurrentExamRequested += OnDeleteCurrentExam;
        _windowViewModel.WindowCloseRequested += (s, e) => Close();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnContinueExam(object? sender, EventArgs e)
    {
        // Check if we're starting a new exam or continuing
        bool startingNewExam = _windowViewModel.IsExamLoadedButNotStarted;
        
        // Update the IsTheExaminationStarted flag
        if (startingNewExam)
        {
            _windowViewModel.MarkExamAsStarted();
        }
        
        _windowViewModel.IsWindowsVisible = false;
        // Open the examination window
        ExaminationWindow window = new ExaminationWindow(null, false);
        window.Closed += (o, args) =>
        {
            Close();
        };
        window.Show();
        // Close(); // Close this dialog when opening the examination window
    }
    
    private async void OnLoadNewExam(object? sender, EventArgs e)
    {
        // Show confirmation dialog if there's an active exam
        if (_windowViewModel.HasActiveExam)
        {
            bool confirmed = await ShowConfirmationDialogAsync(
                _localizationService["exam.dialog.load.new.confirm.title"],
                _localizationService["exam.dialog.load.new.confirm.message"]);
                
            if (!confirmed)
                return;
        }
        
        // Create file picker options
        var options = new FilePickerOpenOptions
        {
            Title = "Select Examination File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Exam Files")
                {
                    Patterns = new[] { "*.json" },
                    MimeTypes = new[] { "application/json" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" },
                    MimeTypes = new[] { "*/*" }
                }
            }
        };

        // Show the file picker
        var result = await StorageProvider.OpenFilePickerAsync(options);
        
        // Check if a file was selected
        if (result.Count > 0)
        {
            var file = result[0];
            var filePath = file.Path.LocalPath;
            
            // Load the examination but don't start it
            _windowViewModel.LoadExamination(filePath);
        }
    }
    
    private async void OnDeleteCurrentExam(object? sender, EventArgs e)
    {
        bool confirmed = await ShowConfirmationDialogAsync(
            _localizationService["exam.dialog.delete.confirm.title"],
            _localizationService["exam.dialog.delete.confirm.message"]);
            
        if (confirmed)
        {
            _windowViewModel.ConfirmDeleteCurrentExam();
        }
    }
    
    private async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        // Create confirmation dialog window
        var dialog = new Window
        {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = 400,
            MinWidth = 400,
            MinHeight = 150,
            MaxWidth = 600,
            MaxHeight = 250,
            TransparencyLevelHint = new[] { Avalonia.Controls.WindowTransparencyLevel.AcrylicBlur },
            ExtendClientAreaToDecorationsHint = true
        };

        // Apply theme service for consistent styling
        var themeService = App.GetService<ThemeService>();
        themeService.ApplyTransparencyToWindow(dialog);

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
            TextAlignment = TextAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        var cancelButton = new Button
        {
            Content = _localizationService["common.cancel"],
            Width = 100,
            Height = 35,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var confirmButton = new Button
        {
            Content = _localizationService["common.confirm"],
            Width = 100,
            Height = 35,
            Classes = { "accent" },
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var result = false;

        cancelButton.Click += (s, e) =>
        {
            result = false;
            dialog.Close();
        };

        confirmButton.Click += (s, e) =>
        {
            result = true;
            dialog.Close();
        };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(confirmButton);

        Grid.SetRow(messageTextBlock, 0);
        Grid.SetRow(buttonPanel, 1);

        grid.Children.Add(messageTextBlock);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;

        await dialog.ShowDialog(this);
        
        return result;
    }

}
