using OpenKnowledge.Interfaces;
using OpenKnowledge.Interfaces.Models;

namespace OpenKnowledge.Log;

public class NullLogger : ILogger
{
    public void Log(LogLevel level, params string[] messages)
    {
        return;
    }

    public void Standard(params string[] messages)
    {
        return;
    }

    public void Formatted(params string[] messages)
    {
        return;
    }
}