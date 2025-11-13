using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class QuestionScore
{
    public string QuestionId { get; set; } = string.Empty;
    public double MaxScore { get; set; } = 0;
    public double ObtainedScore { get; set; } = 0;
    public bool IsCorrect { get; set; } = false;
}
