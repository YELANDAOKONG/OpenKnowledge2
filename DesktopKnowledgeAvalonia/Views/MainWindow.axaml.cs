using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DesktopKnowledgeAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ExaminationWindow window = new();
        window.ShowDialog(this);
    }
}