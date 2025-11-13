using System.Collections.Generic;
using System.IO;
using OpenKnowledge.Log;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = OpenKnowledge.Interfaces.ILogger;

namespace DesktopKnowledge.Services;

public class LoggerService
{
    // Delegate for log level change trigger
    public delegate void LogLevelChangeTrigger(LogLevel oldLevel, LogLevel newLevel, string moduleName);
    
    public ILogger Logger { get; }
    public Microsoft.Extensions.Logging.ILogger Logging { get; }
    
    public string? LogFilePath { get; }
    public string ModuleName { get; }
    
    // Parent reference for hierarchy
    private readonly LoggerService? _parentLogger;
    
    // Track child loggers for propagation
    private readonly List<LoggerService> _childLoggers = new();
    
    // Log level with propagation
    private LogLevel _fileLogLevel = LogLevel.Information;
    public LogLevel FileLogLevel 
    { 
        get => _fileLogLevel;
        set
        {
            if (_fileLogLevel == value)
                return;
                
            var oldLevel = _fileLogLevel;
            _fileLogLevel = value;
            
            // Invoke the trigger if set
            OnLogLevelChanged?.Invoke(oldLevel, value, ModuleName);
            
            // Propagate to children
            PropagateLogLevelChange(value);
        }
    }
    
    // The trigger delegate - renamed to avoid ambiguity
    public LogLevelChangeTrigger? OnLogLevelChanged { get; set; }
    
    private readonly bool _writeToFile;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly RollingInterval  _rollingInterval;
    
    public LoggerService(
        string? logFilePath = null,
        ILogger? logger = null, 
        string? moduleName = null, 
        RollingInterval? rollingInterval = null, 
        ILoggerFactory? loggerFactory = null, 
        bool writeToFile = true,
        LogLevel fileLogLevel = LogLevel.Information,
        LogLevelChangeTrigger? onLogLevelChanged = null,
        LoggerService? parentLogger = null)
    {
        _writeToFile = writeToFile;
        LogFilePath = logFilePath;
        ModuleName = moduleName ?? "Application";
        _fileLogLevel = fileLogLevel;
        OnLogLevelChanged = onLogLevelChanged;
        _parentLogger = parentLogger;
        
        // Register with parent if we have one
        _parentLogger?._childLoggers.Add(this);
        
        if (!string.IsNullOrEmpty(logFilePath))
        {
            string? directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        Logger = logger ?? new ConsoleLogger(ModuleName, true); // "APP"
        
        _loggerFactory = loggerFactory ?? CreateDefaultLoggerFactory(logFilePath, rollingInterval ?? RollingInterval.Infinite);
        // Logging = _loggerFactory.CreateLogger<LoggerService>();
        Logging = _loggerFactory.CreateLogger(ModuleName);
    }
    
    public LoggerService CreateSubModule(string moduleName, bool directName = false)
    {
        var newCustomLoggerName = directName ? moduleName : $"{ModuleName}.{moduleName}";
        var colorful = true;
        if (Logger is ConsoleLogger logger)
        {
            colorful = logger.Colorful;
        }
        var subCustomLogger = new ConsoleLogger(newCustomLoggerName, colorful);
        
        var newName = directName ? moduleName : $"{ModuleName}.{moduleName}";
        
        return new LoggerService(
            logFilePath: LogFilePath,
            logger: subCustomLogger,
            moduleName: newName,
            loggerFactory: _loggerFactory,
            writeToFile: _writeToFile,
            fileLogLevel: FileLogLevel,             // Inherit current log level
            onLogLevelChanged: OnLogLevelChanged,   // Inherit trigger
            parentLogger: this                      // Set parent reference
        );
    }
    
    // Propagate log level changes to all child loggers
    private void PropagateLogLevelChange(LogLevel newLevel)
    {
        foreach (var child in _childLoggers)
        {
            // This will trigger the setter which handles the notification
            child.FileLogLevel = newLevel;
        }
    }
    
    // Static access to update all loggers from any module
    public void UpdateGlobalLogLevel(LogLevel newLevel)
    {
        // Find root logger and update it (propagates to all)
        LoggerService rootLogger = this;
        while (rootLogger._parentLogger != null)
            rootLogger = rootLogger._parentLogger;
        
        rootLogger.FileLogLevel = newLevel;
    }
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger<T>()
    {
        return _loggerFactory?.CreateLogger<T>() ?? Logging;
    }
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger(string categoryName)
    {
        return _loggerFactory?.CreateLogger(categoryName) ?? Logging;
    }
    
    public static ILoggerFactory CreateDefaultLoggerFactory(string? logFilePath, RollingInterval rollingInterval = RollingInterval.Infinite)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            if (!string.IsNullOrEmpty(logFilePath))
            {
                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(logFilePath, 
                        rollingInterval: rollingInterval,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                    .CreateLogger();
                    
                builder.AddSerilog(serilogLogger, dispose: true);
            }
            else
            {
                builder.AddDebug();
            }
        });
    }
    
    private Microsoft.Extensions.Logging.ILogger CreateDefaultMicrosoftLogger(string? logFilePath, RollingInterval rollingInterval = RollingInterval.Infinite)
    {
        var factory = CreateDefaultLoggerFactory(logFilePath, rollingInterval);
        return factory.CreateLogger<LoggerService>();
    }
    
    public void Standard(params string[] messages)
    {
        Logger.Standard(messages);
        // LogToFileIfEnabled(LogLevel.Trace, messages);
    }
    
    public void Formatted(params string[] messages)
    {
        Logger.Formatted(messages);
        // LogToFileIfEnabled(LogLevel.Trace, messages);
    }
    
    public void Null(params string[] messages)
    {
        Logger.Null(messages);
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

    public void Information(params string[] messages)
    {
        Logger.Information(messages);
        LogToFileIfEnabled(LogLevel.Information, messages);
    }

    public void Warning(params string[] messages)
    {
        Logger.Warning(messages);
        LogToFileIfEnabled(LogLevel.Warning, messages);
    }

    public void Error(params string[] messages)
    {
        Logger.Error(messages);
        LogToFileIfEnabled(LogLevel.Error, messages);
    }

    public void Critical(params string[] messages)
    {
        Logger.Critical(messages);
        LogToFileIfEnabled(LogLevel.Critical, messages);
    }
    
    private void LogToFileIfEnabled(LogLevel level, params string[] messages)
    {
        if (!_writeToFile) return;
        
        string message = string.Join(" ", messages);
        switch (level)
        {
            case LogLevel.None:
                break;
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
