using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class InitializationWindow : AppWindowBase
{
    private readonly InitializationViewModel _viewModel;
    private bool _configurationSaved = false;
    
    public InitializationWindow()
    {
        InitializeComponent();
        
        _viewModel = new InitializationViewModel();
        DataContext = _viewModel;
        
        _viewModel.SaveCompleted += (s, e) => 
        {
            _configurationSaved = true;
            Close();
        };
    }
    
    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        // If we haven't completed the configuration, exit the application
        // unless we explicitly closed with a successful save
        if (!_configurationSaved && !_viewModel.IsConfigurationComplete)
        {
            Environment.Exit(0);
        }
    }
    
    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}