using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledge.ViewModels;

namespace DesktopKnowledge.Views;

public partial class StatisticsWindow : AppWindowBase
{
    public StatisticsWindow()
    {
        InitializeComponent();
        var model = new StatisticsWindowViewModel();
        DataContext = model;
        model.WindowCloseRequested += (s, e) => Close();
    }
    
    public StatisticsWindow(StatisticsWindowViewModel model)
    {
        InitializeComponent();
        model.WindowCloseRequested += (s, e) => Close();
        DataContext = model;
    }
}