using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class Question
{
    public string? QuestionId { get; set; } = null;
    public QuestionTypes Type { get; set; } = QuestionTypes.Unknown;
    public string Stem { get; set; } = "Default"; // 题目文本
    public List<(string, string)>? Options { get; set; } = new List<(string, string)>();
    public double Score { get; set; } = 1.0; // 分数

    public string[]? UserAnswer; // 用户答案
    public string[] Answer; // 标准答案
    public string[]? ReferenceAnswer; // 参考答案
    
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { }; // 参考资料
    
    public bool IsAiJudge { get; set; } = false; // 是否需要AI判题
    public string[]? Commits { get; set; } = new string[] { }; // AI提示
    
    public Question[]? SubQuestions { get; set; } = new Question[] { };
}