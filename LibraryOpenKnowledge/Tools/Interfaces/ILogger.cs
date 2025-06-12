namespace LibraryOpenKnowledge.Tools.Interfaces;

public interface ISimpleLogger
{
    public void Log(string level, params string[] messages);
    
    public void Off(params string[] messages);
    public void Trace(params string[] messages);
    public void Debug(params string[] messages);
    public void Info(params string[] messages);
    public void Warn(params string[] messages);
    public void Error(params string[] messages);
    public void Fatal(params string[] messages);
    public void All(params string[] messages);
}