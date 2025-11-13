using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace DesktopKnowledge.Services;

public static class CommandHelper
{
    public static readonly AttachedProperty<ICommand> SelectQuestionCommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand>(
            "SelectQuestionCommand", 
            typeof(CommandHelper));
            
    public static void SetSelectQuestionCommand(Control element, ICommand value) =>
        element.SetValue(SelectQuestionCommandProperty, value);
        
    public static ICommand GetSelectQuestionCommand(Control element) =>
        element.GetValue(SelectQuestionCommandProperty);
}
