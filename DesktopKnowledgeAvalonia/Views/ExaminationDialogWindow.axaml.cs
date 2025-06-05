namespace DesktopKnowledgeAvalonia.Views;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DesktopKnowledgeAvalonia.ViewModels;
using DesktopKnowledgeAvalonia.Services;

public partial class ExaminationDialogWindow : AppWindowBase
{
    private readonly ExaminationDialogViewModel _viewModel;
    
    public ExaminationDialogWindow()
    {
        InitializeComponent();
        
        // Create and set the view model
        var configService = App.GetService<ConfigureService>();
        var localizationService = App.GetService<LocalizationService>();
        
        _viewModel = new ExaminationDialogViewModel(configService, localizationService);
        DataContext = _viewModel;
        
        // Subscribe to events
        _viewModel.CloseRequested += (s, e) => Close();
        _viewModel.ContinueExamRequested += OnContinueExam;
        _viewModel.LoadNewExamRequested += OnLoadNewExam;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnContinueExam(object? sender, EventArgs e)
    {
        // Open the examination window with the current examination
        ExaminationWindow window = new ExaminationWindow(null, false);
        window.Show();
        Close(); // Close this dialog when opening the examination window
    }
    
    private async void OnLoadNewExam(object? sender, EventArgs e)
    {
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
            
            // Create and show the examination window
            ExaminationWindow window = new ExaminationWindow(filePath, true);
            window.Show();
            Close(); // Close this dialog when opening the examination window
        }
    }
}
