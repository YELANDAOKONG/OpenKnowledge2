using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class StudyWindow : Window
{
    public StudyWindow(StudyWindowViewModel model)
    {
        InitializeComponent();
        DataContext = model;
    }
}