using System.Runtime.Serialization;
using LibraryOpenKnowledge.Tools;
using Newtonsoft.Json;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class Question
{
    public string? QuestionId { get; set; } = null;
    public QuestionTypes Type { get; set; } = QuestionTypes.Unknown;
    public string Stem { get; set; } = "Default"; // 题目文本
    [JsonProperty(ItemConverterType = typeof(OptionConverter))]
    public List<Option> Options { get; set; } = new List<Option>(); // public List<(string, string)>? Options { get; set; } = new List<(string, string)>();
    public double Score { get; set; } = 1.0; // 分数

    public string[]? UserAnswer { get; set; } // 用户答案
    public string[] Answer { get; set; } // 标准答案
    public string[]? ReferenceAnswer { get; set; } // 参考答案
    
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { }; // 参考资料
    
    public bool IsAiJudge { get; set; } = false; // 是否需要AI判题
    public string[]? Commits { get; set; } = new string[] { }; // AI提示
    
    // public Question[]? SubQuestions { get; set; } = new Question[] { };
    public List<Question>? SubQuestions { get; set; } = new List<Question>();
    
    
    public double? ObtainedScore { get; set; } = null; // 获得的分数
    public bool IsAiEvaluated { get; set; } = true; // 是否已评估（对于 AI 题目，默认为 false 直到AI评估完成）
    public string? AiFeedback { get; set; } = null; // AI评估反馈
    
}