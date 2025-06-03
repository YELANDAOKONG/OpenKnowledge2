using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ExaminationWindowViewModel model = new();
        ExaminationWindow window = new(model);
        window.ShowDialog(this);
    }

    private void ThrowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new Exception("Test Exception");
    }

    private void StudyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        StudyWindowViewModel model = new();
        StudyWindow window = new(model);
        window.ShowDialog(this);
    }
}