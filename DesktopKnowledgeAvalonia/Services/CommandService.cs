using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace DesktopKnowledgeAvalonia.Services;

public static class CommandService
{
    public static readonly AttachedProperty<ICommand> CommandProperty = 
        AvaloniaProperty.RegisterAttached<Control, ICommand>("Command", typeof(CommandService));
        
    public static void SetCommand(Control element, ICommand value) => 
        element.SetValue(CommandProperty, value);
        
    public static ICommand GetCommand(Control element) => 
        element.GetValue(CommandProperty);
}