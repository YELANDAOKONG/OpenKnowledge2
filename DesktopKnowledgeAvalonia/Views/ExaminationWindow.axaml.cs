namespace DesktopKnowledgeAvalonia.Views;

using Avalonia.Controls;
using DesktopKnowledgeAvalonia.ViewModels;
using System;

public partial class ExaminationWindow : AppWindowBase
{
    private readonly ExaminationWindowViewModel _viewModel;

    public ExaminationWindow()
    {
        InitializeComponent();
        _viewModel = new ExaminationWindowViewModel();
    }

    public ExaminationWindow(ExaminationWindowViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // _viewModel.ExitRequested += (s, e) => Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
    }
}