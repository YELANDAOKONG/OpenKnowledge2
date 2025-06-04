using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DesktopKnowledgeAvalonia.Models;
using DesktopKnowledgeAvalonia.Services;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class MainWindow : AppWindowBase
{
    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
        var model = new MainWindowViewModel();
        model.WindowCloseRequested += (s, e) => Close();
        DataContext = model;
    }
    
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        viewModel.WindowCloseRequested += (s, e) => Close();
        DataContext = viewModel;
    }

    private void OnUsernameDoubleTapped(object sender, TappedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsEditingUsername = true;
        }
    }

    private void OnUsernameEditComplete(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsEditingUsername = false;
            ViewModel.SaveUsername();
        }
    }

    private void OnUsernameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            if (ViewModel != null)
            {
                ViewModel.IsEditingUsername = false;
                if (e.Key == Key.Enter)
                {
                    ViewModel.SaveUsername();
                }
            }
        }
    }
}