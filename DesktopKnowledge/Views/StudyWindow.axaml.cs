using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledge.ViewModels;

namespace DesktopKnowledge.Views;

public partial class StudyWindow : AppWindowBase
{
    public StudyWindow(StudyWindowViewModel model)
    {
        InitializeComponent();
        DataContext = model;
    }
}