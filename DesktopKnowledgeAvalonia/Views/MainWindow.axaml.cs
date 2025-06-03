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
        ExaminationWindow window = new();
        window.ShowDialog(this);
    }

    private void ThrowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}