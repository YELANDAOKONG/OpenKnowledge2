using LibraryOpenKnowledge.Interfaces.Models;

namespace LibraryOpenKnowledge.Interfaces;

public interface ILogger
{
    void Log(LogLevel level, params string[] messages);
    
    void Standard(params string[] messages);
    void Formatted(params string[] messages);
    
    void Null(params string[] messages) => Log(LogLevel.Null, messages);
    void Trace(params string[] messages) => Log(LogLevel.Trace, messages);
    void Debug(params string[] messages) => Log(LogLevel.Debug, messages);
    void Information(params string[] messages) => Log(LogLevel.Information, messages);
    void Warning(params string[] messages) => Log(LogLevel.Warning, messages);
    void Error(params string[] messages) => Log(LogLevel.Error, messages);
    void Critical(params string[] messages) => Log(LogLevel.Critical, messages);
}