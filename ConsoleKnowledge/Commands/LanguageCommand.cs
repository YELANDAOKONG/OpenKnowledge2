using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ConsoleKnowledge.I18n;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleKnowledge.Commands;

public class LanguageCommand : AsyncCommand<LanguageCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-l|--list")]
        [Description("List all available languages")]
        public bool ListLanguages { get; set; }
        
        [CommandOption("-s|--set <LANG>")]
        [Description("Set the application language")]
        public string? Language { get; set; }
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var i18n = I18nService.Instance;
        
        if (settings.ListLanguages)
        {
            AnsiConsole.MarkupLine("[blue]Available languages:[/]");
            foreach (var lang in i18n.GetSupportedLanguages())
            {
                var isCurrent = lang == i18n.CurrentLanguage;
                AnsiConsole.MarkupLine($"{(isCurrent ? "[green]* " : "  ")}{lang}[/]");
            }
            return 0;
        }
        
        if (!string.IsNullOrEmpty(settings.Language))
        {
            try
            {
                i18n.CurrentLanguage = settings.Language;
                AnsiConsole.MarkupLine($"[green]Language set to '{settings.Language}'[/]");
                return 0;
            }
            catch (KeyNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Language '{settings.Language}' is not supported");
                AnsiConsole.MarkupLine("[blue]Available languages:[/]");
                foreach (var lang in i18n.GetSupportedLanguages())
                {
                    AnsiConsole.MarkupLine($"  {lang}");
                }
                return 1;
            }
        }
        
        // 如果没有指定参数，显示当前语言
        AnsiConsole.MarkupLine($"[blue]Current language:[/] {i18n.CurrentLanguage}");
        AnsiConsole.MarkupLine("[gray]Use --list to see all available languages[/]");
        AnsiConsole.MarkupLine("[gray]Use --set to change the language[/]");
        
        return 0;
    }
}
