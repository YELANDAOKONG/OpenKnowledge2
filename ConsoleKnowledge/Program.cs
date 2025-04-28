using ConsoleKnowledge.Commands;
using ConsoleKnowledge.I18n;
using LibraryOpenKnowledge;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleKnowledge;

class Program
{
    static int Main(string[] args)
    {
        // 初始化国际化服务
        var i18n = I18nService.Instance;
        
        // 如果命令行参数中有语言设置，则应用它
        for (int i = 0; i < args.Length - 1; i++)
        {
            if ((args[i] == "--lang" || args[i] == "-l") && i + 1 < args.Length)
            {
                try
                {
                    i18n.CurrentLanguage = args[i + 1];
                    
                    // 从args中移除这两个参数，避免影响命令行解析
                    var newArgs = args.ToList();
                    newArgs.RemoveAt(i);
                    newArgs.RemoveAt(i); // 移除后，原来i+1的元素现在是i
                    args = newArgs.ToArray();
                    
                    break;
                }
                catch (KeyNotFoundException)
                {
                    AnsiConsole.MarkupLine($"[red]Language '{args[i + 1]}' is not supported. Using default language.[/]");
                }
            }
        }
        
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<ExamCommand>("exam")
                .WithDescription("Load and answer an examination file")
                .WithExample(new[] { "exam", "--file", "exam.json" });
                
            // 添加一个新的命令用于显示和设置语言
            config.AddCommand<LanguageCommand>("lang")
                .WithDescription("Display or set the application language")
                .WithExample(new[] { "lang", "--list" })
                .WithExample(new[] { "lang", "--set", "zh-CN" });
        });
        
        try
        {
            return app.Run(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][[!]] Error:[/] {ex.Message}");
            return 1;
        }
    }
}
