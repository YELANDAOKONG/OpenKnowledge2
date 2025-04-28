using ConsoleKnowledge.Commands;
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
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<ExamCommand>("exam")
                .WithDescription("Load and answer an examination file")
                .WithExample(new[] { "exam", "--file", "exam.json" });
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