using System.IO;
using System.Collections.Generic;
using LibraryOpenKnowledge.Tools;
using Microsoft.Extensions.Logging;
using Serilog;
using CustomLogger = LibraryOpenKnowledge.Tools.Interfaces.ISimpleLogger;

namespace DesktopKnowledgeAvalonia.Services;

public class LoggerService
{
    // Delegate for log level change trigger
    public delegate void LogLevelChangeTrigger(LogLevel oldLevel, LogLevel newLevel, string moduleName);
    
    public CustomLogger Logger { get; }
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
    
    public LoggerService(
        string? logFilePath = null, 
        CustomLogger? logger = null, 
        string? moduleName = null, 
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
                Directory.CreateDirectory(directory);
        }
        
        Logger = logger ?? new ConsoleSimpleLogger(ModuleName, true);
        
        _loggerFactory = loggerFactory ?? CreateDefaultLoggerFactory(logFilePath);
        Logging = _loggerFactory.CreateLogger(ModuleName);
    }
    
    public LoggerService CreateSubModule(string moduleName, bool directName = false)
    {
        var newCustomLoggerName = directName ? moduleName : $"{ModuleName}.{moduleName}";
        var subCustomLogger = new ConsoleSimpleLogger(newCustomLoggerName, true);
        
        var newName = directName ? moduleName : $"{ModuleName}.{moduleName}";
        
        // Create submodule with current logger as parent
        return new LoggerService(
            logFilePath: LogFilePath,
            logger: subCustomLogger,
            moduleName: newName,
            loggerFactory: _loggerFactory,
            writeToFile: _writeToFile,
            fileLogLevel: FileLogLevel,        // Inherit current log level
            onLogLevelChanged: OnLogLevelChanged,  // Inherit trigger
            parentLogger: this                // Set parent reference
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
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger<T>() => 
        _loggerFactory?.CreateLogger<T>() ?? Logging;
    
    public Microsoft.Extensions.Logging.ILogger GetSubLogger(string categoryName) => 
        _loggerFactory?.CreateLogger(categoryName) ?? Logging;
    
    public static ILoggerFactory CreateDefaultLoggerFactory(string? logFilePath)
    {
        return LoggerFactory.Create(builder =>
        {
            // Set to minimum to allow filtering at the call site
            builder.SetMinimumLevel(LogLevel.Trace);
            
            if (!string.IsNullOrEmpty(logFilePath))
            {
                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose() // Allow all levels and filter at the call site
                    .WriteTo.File(logFilePath, 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                    .CreateLogger();
                    
                builder.AddSerilog(serilogLogger, dispose: true);
            }
            else
                builder.AddDebug();
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
        // Skip if file logging is disabled or level is below threshold
        if (!_writeToFile || level < FileLogLevel) 
            return;
        
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
