using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class QuestionScore : ISerializable
{
    public string QuestionId { get; set; } = string.Empty;
    public double MaxScore { get; set; } = 0;
    public double ObtainedScore { get; set; } = 0;
    public bool IsCorrect { get; set; } = false;
    
    #region ISerializable
    
    public QuestionScore() { }
    
    protected QuestionScore(SerializationInfo info, StreamingContext context)
    {
        QuestionId = info.GetString("QuestionId") ?? string.Empty;
        MaxScore = info.GetDouble("MaxScore");
        ObtainedScore = info.GetDouble("ObtainedScore");
        IsCorrect = info.GetBoolean("IsCorrect");
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("QuestionId", QuestionId);
        info.AddValue("MaxScore", MaxScore);
        info.AddValue("ObtainedScore", ObtainedScore);
        info.AddValue("IsCorrect", IsCorrect);
    }
    
    #endregion
}
