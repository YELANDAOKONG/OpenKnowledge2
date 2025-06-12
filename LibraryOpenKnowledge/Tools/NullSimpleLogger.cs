using LibraryOpenKnowledge.Tools.Interfaces;

namespace LibraryOpenKnowledge.Tools;

public class NullSimpleLogger : ISimpleLogger
{
    public void Log(string level, params string[] messages)
    {
        return;
    }

    public void Off(params string[] messages)
    {
        return;
    }

    public void Trace(params string[] messages)
    {
        return;
    }

    public void Debug(params string[] messages)
    {
        return;
    }

    public void Info(params string[] messages)
    {
        return;
    }

    public void Warn(params string[] messages)
    {
        return;
    }

    public void Error(params string[] messages)
    {
        return;
    }

    public void Fatal(params string[] messages)
    {
        return;
    }

    public void All(params string[] messages)
    {
        return;
    }
}