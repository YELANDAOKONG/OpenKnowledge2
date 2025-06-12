using System.IO;
using LibraryOpenKnowledge.Tools;
using Microsoft.Extensions.Logging;
using Serilog;
using CustomLogger = LibraryOpenKnowledge.Tools.Interfaces.ISimpleLogger;

namespace DesktopKnowledgeAvalonia.Services;

public class LoggerService
{
    public CustomLogger Logger { get; }
    public Microsoft.Extensions.Logging.ILogger Logging { get; }
    
    public string? LogFilePath { get; }
    
    private readonly bool _writeToFile;
    private readonly ILoggerFactory? _loggerFactory;
    
    public LoggerService(string? logFilePath = null, CustomLogger? logger = null, ILoggerFactory? loggerFactory = null, bool writeToFile = true)
    {
        _writeToFile = writeToFile;
        LogFilePath = logFilePath;
        
        if (!string.IsNullOrEmpty(logFilePath))
        {
            string? directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        Logger = logger ?? new ConsoleSimpleLogger("APP", true);
        
        _loggerFactory = loggerFactory ?? CreateDefaultLoggerFactory(logFilePath);
        Logging = _loggerFactory.CreateLogger<LoggerService>();
    }
    
    public LoggerService CreateSubModule(string moduleName)
    {
        var subCustomLogger = Logger is ConsoleSimpleLogger consoleLogger 
            ? new ConsoleSimpleLogger($"{consoleLogger.Title}.{moduleName}", consoleLogger.Colorful)
            : new ConsoleSimpleLogger($"APP.{moduleName}", true);
        
        return new LoggerService(
            logFilePath: LogFilePath,
            logger: subCustomLogger,
            loggerFactory: _loggerFactory,
            writeToFile: _writeToFile
        );
    }
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger<T>()
    {
        return _loggerFactory?.CreateLogger<T>() ?? Logging;
    }
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger(string categoryName)
    {
        return _loggerFactory?.CreateLogger(categoryName) ?? Logging;
    }
    
    public static ILoggerFactory CreateDefaultLoggerFactory(string? logFilePath)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            if (!string.IsNullOrEmpty(logFilePath))
            {
                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(logFilePath, 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                    .CreateLogger();
                    
                builder.AddSerilog(serilogLogger, dispose: true);
            }
            else
            {
                builder.AddDebug();
            }
        });
    }
    
    private Microsoft.Extensions.Logging.ILogger CreateDefaultMicrosoftLogger(string? logFilePath)
    {
        var factory = CreateDefaultLoggerFactory(logFilePath);
        return factory.CreateLogger<LoggerService>();
    }
    
    public void All(params string[] messages)
    {
        Logger.All(messages);
        LogToFileIfEnabled(LogLevel.Trace, messages);
    }
    
    public void Trace(params string[] messages)
    {
        Logger.Trace(messages);
        LogToFileIfEnabled(LogLevel.Trace, messages);
    }

    public void Debug(params string[] messages)
    {
        Logger.Debug(messages);
        LogToFileIfEnabled(LogLevel.Debug, messages);
    }

    public void Info(params string[] messages)
    {
        Logger.Info(messages);
        LogToFileIfEnabled(LogLevel.Information, messages);
    }

    public void Warn(params string[] messages)
    {
        Logger.Warn(messages);
        LogToFileIfEnabled(LogLevel.Warning, messages);
    }

    public void Error(params string[] messages)
    {
        Logger.Error(messages);
        LogToFileIfEnabled(LogLevel.Error, messages);
    }

    public void Fatal(params string[] messages)
    {
        Logger.Fatal(messages);
        LogToFileIfEnabled(LogLevel.Critical, messages);
    }

    public void Off(params string[] messages)
    {
        Logger.Off(messages);
    }
    
    private void LogToFileIfEnabled(LogLevel level, params string[] messages)
    {
        if (!_writeToFile) return;
        
        string message = string.Join(" ", messages);
        switch (level)
        {
            case LogLevel.Trace:
                Logging.LogTrace(message);
                break;
            case LogLevel.Debug:
                Logging.LogDebug(message);
                break;
            case LogLevel.Information:
                Logging.LogInformation(message);
                break;
            case LogLevel.Warning:
                Logging.LogWarning(message);
                break;
            case LogLevel.Error:
                Logging.LogError(message);
                break;
            case LogLevel.Critical:
                Logging.LogCritical(message);
                break;
        }
    }
}
