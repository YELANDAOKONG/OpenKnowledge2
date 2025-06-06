using System;

namespace DesktopKnowledgeAvalonia.Utils;

public static class TimeUtil
{
    public static long GetUnixTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
    
    public static long GetUnixTimestampMilliseconds()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }
    
    public static int GetYear()
    {
        return DateTime.UtcNow.Year;
    }
    
    public static int GetMonth()
    {
        return DateTime.UtcNow.Month;
    }
    
    public static int GetDay()
    {
        return DateTime.UtcNow.Day;
    }
}