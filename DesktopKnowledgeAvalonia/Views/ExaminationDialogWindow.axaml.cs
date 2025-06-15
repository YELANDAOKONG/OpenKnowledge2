using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DesktopKnowledgeAvalonia.ViewModels;
using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.Views;

public partial class ExaminationDialogWindow : AppWindowBase
{
    private readonly ExaminationDialogWindowViewModel _windowViewModel;
    private readonly LocalizationService _localizationService;
    
    private readonly LoggerService _logger;
    private Border? _dragDropOverlay;
    private Grid? _mainGrid;
    
    public ExaminationDialogWindow()
    {
        InitializeComponent();
        
        // Create and set the view model
        var configService = App.GetService<ConfigureService>();
        _localizationService = App.GetService<LocalizationService>();
        _logger = App.GetWindowsLogger("ExaminationDialogWindow");
        
        _windowViewModel = new ExaminationDialogWindowViewModel(configService, _localizationService);
        DataContext = _windowViewModel;
        
        // Subscribe to events
        _windowViewModel.CloseRequested += (s, e) => Close();
        _windowViewModel.ContinueExamRequested += OnContinueExam;
        _windowViewModel.LoadNewExamRequested += OnLoadNewExam;
        _windowViewModel.DeleteCurrentExamRequested += OnDeleteCurrentExam;
        _windowViewModel.WindowCloseRequested += (s, e) => Close();
        
        // Setup drag and drop
        SetupDragAndDrop();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void SetupDragAndDrop()
    {
        try
        {
            // Get references to controls
            _dragDropOverlay = this.FindControl<Border>("DragDropOverlay");
            _mainGrid = this.FindControl<Grid>("MainGrid");
            
            if (_mainGrid != null)
            {
                // Enable drag and drop on the main grid
                DragDrop.SetAllowDrop(_mainGrid, true);
                
                // Subscribe to drag and drop events
                _mainGrid.AddHandler(DragDrop.DragOverEvent, OnDragOver);
                _mainGrid.AddHandler(DragDrop.DropEvent, OnDrop);
                _mainGrid.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
                
                _logger.Debug("Drag and drop setup completed");
            }
            else
            {
                _logger.Warn("MainGrid not found - drag and drop will not work");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error setting up drag and drop: {ex.Message}");
            _logger.Trace($"Error setting up drag and drop: {ex.StackTrace}");
        }
    }
    
    #region Drag and Drop Event Handlers
    
    /// <summary>
    /// Handles the drag over event to validate dropped files
    /// </summary>
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        try
        {
            // Check if the drag data contains files
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    // Check if any of the files is a JSON file
                    var jsonFiles = files.Where(file => 
                        !string.IsNullOrEmpty(file.Name) &&
                        Path.GetExtension(file.Name).Equals(".json", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                        
                    if (jsonFiles.Any())
                    {
                        // Allow drop for JSON files
                        e.DragEffects = DragDropEffects.Copy;
                        ShowDragDropOverlay(true);
                        _logger.Debug($"Drag over: Found {jsonFiles.Count} JSON file(s)");
                    }
                    else
                    {
                        // Don't allow drop for non-JSON files
                        e.DragEffects = DragDropEffects.None;
                        ShowDragDropOverlay(false);
                        _logger.Debug("Drag over: No JSON files found");
                    }
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                    ShowDragDropOverlay(false);
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
                ShowDragDropOverlay(false);
            }
            
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in drag over event: {ex.Message}");
            _logger.Trace($"Error in drag over event: {ex.StackTrace}");
            
            e.DragEffects = DragDropEffects.None;
            ShowDragDropOverlay(false);
            e.Handled = true;
        }
    }
    
    /// <summary>
    /// Handles the drop event to process dropped files
    /// </summary>
    private async void OnDrop(object? sender, DragEventArgs e)
    {
        try
        {
            // Hide the drag drop overlay
            ShowDragDropOverlay(false);
            
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    // Get the first JSON file
                    var jsonFile = files.FirstOrDefault(file => 
                        !string.IsNullOrEmpty(file.Name) &&
                        Path.GetExtension(file.Name).Equals(".json", StringComparison.OrdinalIgnoreCase));
                        
                    if (jsonFile != null)
                    {
                        var filePath = jsonFile.Path.LocalPath;
                        _logger.Info($"File dropped: {filePath}");
                        
                        // Check if we need to show confirmation dialog
                        bool shouldLoad = true;
                        if (_windowViewModel.HasActiveExam)
                        {
                            shouldLoad = await ShowConfirmationDialogAsync(
                                _localizationService["exam.dialog.load.new.confirm.title"],
                                _localizationService["exam.dialog.load.new.confirm.message"]);
                        }
                        
                        if (shouldLoad)
                        {
                            // Load the examination
                            _windowViewModel.LoadExamination(filePath);
                            _logger.Info($"Examination loaded from dropped file: {filePath}");
                        }
                        else
                        {
                            _logger.Info("User cancelled loading examination from dropped file");
                        }
                    }
                    else
                    {
                        // Show error message for invalid file type
                        _windowViewModel.ShowTemporaryStatusMessage(_localizationService["exam.dialog.drop.invalid.file"]);
                        _logger.Warn("Invalid file type dropped - not a JSON file");
                    }
                }
            }
            
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error handling dropped file: {ex.Message}");
            _logger.Trace($"Error handling dropped file: {ex.StackTrace}");
            
            // Show error message
            _windowViewModel.ShowTemporaryStatusMessage(_localizationService["exam.dialog.drop.error"]);
            e.Handled = true;
        }
    }
    
    /// <summary>
    /// Handles drag leave event to hide overlay
    /// </summary>
    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        try
        {
            ShowDragDropOverlay(false);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in drag leave event: {ex.Message}");
            _logger.Trace($"Error in drag leave event: {ex.StackTrace}");
            e.Handled = true;
        }
    }
    
    /// <summary>
    /// Shows or hides the drag drop overlay
    /// </summary>
    private void ShowDragDropOverlay(bool show)
    {
        try
        {
            if (_dragDropOverlay != null)
            {
                _dragDropOverlay.IsVisible = show;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error showing drag drop overlay: {ex.Message}");
            _logger.Trace($"Error showing drag drop overlay: {ex.StackTrace}");
        }
    }
    
    #endregion
    
    private void OnContinueExam(object? sender, EventArgs e)
    {
        // Check if we're starting a new exam or continuing
        bool startingNewExam = _windowViewModel.IsExamLoadedButNotStarted;
        
        // Update the IsTheExaminationStarted flag
        if (startingNewExam)
        {
            _windowViewModel.MarkExamAsStarted();
        }

        var config = App.GetService<ConfigureService>();
        config.AppStatistics.AddLoadExaminationCount(config);
        
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
            _logger.Info($"Loading examination from file: {filePath}");
            
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
        try
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
        catch (Exception ex)
        {
            _logger?.Error($"Error showing confirmation dialog: {ex.Message}");
            _logger?.Trace($"Error showing confirmation dialog: {ex.StackTrace}");
            return false; // 出错时默认取消
        }
    }

}
