using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ConsoleKnowledge.Core;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleKnowledge.Commands;

public class ExamCommand : AsyncCommand<ExamCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-f|--file <FILE>")]
        [Description("The path to the examination file.")]
        public string FilePath { get; set; } = string.Empty;
    }
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.FilePath))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] File path is required.");
            return 1;
        }
        try
        {
            var examManager = new ExamManager(settings.FilePath);
            await examManager.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}