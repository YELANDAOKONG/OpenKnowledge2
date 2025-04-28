using ApiKnowledge.Utils;

namespace ApiKnowledge.Models;

[Serializable]
public class JsonBuilder
{
    public int Status = 0;
    public string Message = String.Empty;
    public long Time = 0;
    public string Guid = System.Guid.NewGuid().ToString();
    public object? Data = null;

    public JsonBuilder(){}
    
    public JsonBuilder(int status, string message, object? data = null)
    {
        this.Status = status;
        this.Message = message;
        this.Time = TimeUtils.GetTimestampMilliseconds();
        this.Guid = System.Guid.NewGuid().ToString();
        this.Data = data;
    }

    public JsonBuilder(
        int status,
        string message,
        long time,
        string guid,
        object? data = null
    )
    {
        this.Status = status;
        this.Message = message;
        this.Time = time;
        this.Guid = guid;
        this.Data = data;
    }
}