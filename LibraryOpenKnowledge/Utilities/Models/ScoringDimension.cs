namespace LibraryOpenKnowledge.Utilities.Models;

// 用于维度评分的类
[Serializable]
public class ScoringDimension
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public double MaxScore { get; set; }
}
