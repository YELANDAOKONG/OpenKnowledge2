using DesktopKnowledgeAvalonia.Services;

namespace DesktopKnowledgeAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ConfigureService Configure;
    
    public MainWindowViewModel()
    {
        Configure = App.GetService<ConfigureService>();
    }
    
    public string Greeting { get; } = "Hello World!";
}