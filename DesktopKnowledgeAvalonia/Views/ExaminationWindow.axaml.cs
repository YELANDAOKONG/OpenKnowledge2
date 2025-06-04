using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DesktopKnowledgeAvalonia.ViewModels;

namespace DesktopKnowledgeAvalonia.Views;

public partial class ExaminationWindow : AppWindowBase
{
    public ExaminationWindow(ExaminationWindowViewModel model)
    {
        InitializeComponent();
        this.DataContext = model;
    }
}