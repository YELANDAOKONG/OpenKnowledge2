using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class SettingWindow : AppWindowBase
{
    public SettingWindow()
    {
        InitializeComponent();
        var model = new SettingWindowViewModel();
        DataContext = model;
        model.WindowCloseRequested += (s, e) => Close();
    }
    
    public SettingWindow(SettingWindowViewModel model)
    {
        InitializeComponent();
        model.WindowCloseRequested += (s, e) => Close();
        DataContext = model;
    }
}