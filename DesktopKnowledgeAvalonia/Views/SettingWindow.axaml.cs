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
        DataContext = new SettingWindowViewModel();
    }
    
    public SettingWindow(SettingWindowViewModel model)
    {
        InitializeComponent();
        DataContext = model;
    }
}