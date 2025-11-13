using System.Text;
using OpenKnowledge.Interfaces;
using OpenKnowledge.Interfaces.Models;
using Spectre.Console;

namespace OpenKnowledge.Log;

public class ConsoleLogger : ILogger
{
    public string Title { get; set; } = "APP";
    public bool Colorful { get; set; } = true;
    public readonly object ConsoleLock = new();
    
    public ConsoleLogger() { }
    
    public ConsoleLogger(bool colorful = true)
    {
        Colorful = colorful;
    }
    
    public ConsoleLogger(string title, bool colorful = true)
    {
        Title = title;
        Colorful = colorful;
    }

    public void Log(LogLevel level, params string[] messages)
    {
        if (level == LogLevel.Null) return;
        
        var (levelString, levelColor) = GetLevelInfo(level);
        if (Colorful)
        {
            LogColorful(levelString, messages, levelColor);
        }
        else
        {
            LogNormal(levelString, messages);
        }
    }

    private (string levelString, string levelColor) GetLevelInfo(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ("T", "purple"),
            LogLevel.Debug => ("D", "blue"),
            LogLevel.Information => ("I", "white"),
            LogLevel.Warning => ("W", "yellow"),
            LogLevel.Error => ("E", "red"),
            LogLevel.Critical => ("C", "red"),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported log level")
        };
    }
    
    private void LogNormal(string level, string[] messages)
    {
        var builder = new StringBuilder()
            .Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ")
            .Append($"[{level}] ")
            .Append($"({Title}) ");

        foreach (var message in messages)
        {
            builder.Append(message).Append(' ');
        }

        lock (ConsoleLock)
        {
            Console.WriteLine(builder.ToString());
        }
    }
    
    private void LogColorful(string level, string[] messages, string levelColor)
    {
        var builder = new StringBuilder()
            .Append($"[grey][[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]][/] ")
            .Append($"[{levelColor}][[{Markup.Escape(level)}]][/] ")
            .Append($"[{levelColor}]({Markup.Escape(Title)})[/] ");

        foreach (var message in messages)
        {
            builder.Append($"[{levelColor}]{Markup.Escape(message)}[/] ");
        }

        lock (ConsoleLock)
        {
            AnsiConsole.MarkupLine(builder.ToString());
        }
    }
    
    public void Standard(params string[] messages)
    {
        if (Colorful)
        {
            var builder = new StringBuilder();
            foreach (var message in messages)
            {
                builder.Append($"[grey]{Markup.Escape(message)}[/] ");
            }

            lock (ConsoleLock)
            {
                AnsiConsole.MarkupLine(builder.ToString());
            }
        }
        else
        {
            var builder = new StringBuilder();
            foreach (var message in messages)
            {
                builder.Append(message).Append(' ');
            }

            lock (ConsoleLock)
            {
                Console.WriteLine(builder.ToString());
            }
        }
    }

    public void Formatted(params string[] messages)
    {
        if (Colorful)
        {
            var builder = new StringBuilder();
            foreach (var message in messages)
            {
                builder.Append($"{message} ");
            }

            lock (ConsoleLock)
            {
                AnsiConsole.MarkupLine(builder.ToString());
            }
        }
        else
        {
            var builder = new StringBuilder();
            foreach (var message in messages)
            {
                builder.Append(message).Append(' ');
            }

            lock (ConsoleLock)
            {
                Console.WriteLine(builder.ToString());
            }
        }
    }
    
    public void Null(params string[] messages) => Log(LogLevel.Null, messages);
    public void Trace(params string[] messages) => Log(LogLevel.Trace, messages);
    public void Debug(params string[] messages) => Log(LogLevel.Debug, messages);
    public void Information(params string[] messages) => Log(LogLevel.Information, messages);
    public void Warning(params string[] messages) => Log(LogLevel.Warning, messages);
    public void Error(params string[] messages) => Log(LogLevel.Error, messages);
    public void Critical(params string[] messages) => Log(LogLevel.Critical, messages);
}
