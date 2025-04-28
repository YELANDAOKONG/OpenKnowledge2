namespace ApiKnowledge.Utils;

public class TimeUtils
{
    public static long GetTimestampMilliseconds()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}