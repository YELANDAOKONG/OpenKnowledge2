using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ConsoleKnowledge.Core;
using ConsoleKnowledge.I18n;
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
        
        [CommandOption("-l|--lang <LANG>")]
        [Description("Set the language for this session.")]
        public string? Language { get; set; }
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.FilePath))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] File path is required.");
            return 1;
        }
        
        // 设置语言（如果指定）
        if (!string.IsNullOrEmpty(settings.Language))
        {
            try
            {
                I18nService.Instance.CurrentLanguage = settings.Language;
            }
            catch (KeyNotFoundException)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Language '{settings.Language}' is not supported. Using default language.");
            }
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