using System;

namespace DesktopKnowledge.Utils;

public static class TimeHelper
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

    /// <summary>
    /// 将从0开始的13位毫秒时间戳转换为计时器格式 (00:00:00:00 (DD:HH:MM:SS))
    /// </summary>
    /// <param name="timestamp">时间戳</param>
    /// <param name="daily">是否显示天数</param>
    /// <returns>计时器格式字符串</returns>
    public static string ToTimerString(long timestamp, bool daily = false)
    {
        if (daily)
        {
            return $"{TimeSpan.FromMilliseconds(timestamp):dd\\:hh\\:mm\\:ss}";
        }
        else
        {
            return $"{TimeSpan.FromMilliseconds(timestamp):hh\\:mm\\:ss}";
        }
    }
}