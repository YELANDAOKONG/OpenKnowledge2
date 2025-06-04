using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.Views;

public partial class FatalErrorWindow : AppWindowBase
{
    private readonly LocalizationService _localizationService;
    private readonly string _errorDetails;
    
    public FatalErrorWindow()
    {
        InitializeComponent();
        
        // Get localization service
        _localizationService = App.GetService<LocalizationService>();
        _errorDetails = string.Empty;
    }
    
    public FatalErrorWindow(Exception exception) : this()
    {
        if (exception == null)
            return;
            
        // Format the exception details
        _errorDetails = FormatExceptionDetails(exception);
        ErrorDetailsTextBlock.Text = _errorDetails;
    }
    
    private string FormatExceptionDetails(Exception exception)
    {
        var details = $"{DateTime.Now}\r\n";
        details += $"Exception: {exception.GetType().FullName}\r\n";
        details += $"Message: {exception.Message}\r\n";
        
        if (exception.Source != null)
            details += $"Source: {exception.Source}\r\n";
            
        if (exception.TargetSite != null)
            details += $"Method: {exception.TargetSite}\r\n";
            
        details += "\r\nStack Trace:\r\n";
        details += exception.StackTrace ?? "No stack trace available";
        
        // Include inner exception if available
        if (exception.InnerException != null)
        {
            details += "\r\n\r\nInner Exception:\r\n";
            details += $"Type: {exception.InnerException.GetType().FullName}\r\n";
            details += $"Message: {exception.InnerException.Message}\r\n";
            details += "\r\nStack Trace:\r\n";
            details += exception.InnerException.StackTrace ?? "No stack trace available";
        }
        
        return details;
    }
    
    private async void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_errorDetails))
            return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            await topLevel.Clipboard!.SetTextAsync(_errorDetails);
        
            // Show a temporary success message
            CopyButton.Content = _localizationService["error.copy.success"];
            await Task.Delay(2000);
        
            // Reset button content
            var buttonStack = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 5 };
            buttonStack.Children.Add(new PathIcon
            {
                Data = Avalonia.Media.Geometry.Parse("M5 3a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H5zm0 2h10v14H5V5zm14-3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-1v-2h1V4H9V3h10z"),
                Width = 16,
                Height = 16
            });
            buttonStack.Children.Add(new TextBlock { Text = _localizationService["error.button.copy"] });
            CopyButton.Content = buttonStack;
        }
    }
    
    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        Environment.Exit(1);
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Environment.Exit(1);
    }

}
