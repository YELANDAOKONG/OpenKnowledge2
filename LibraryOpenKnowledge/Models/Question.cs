using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class Question : ISerializable
{

    public string? QuestionId { get; set; } = null;
    public QuestionTypes Type { get; set; } = QuestionTypes.Unknown;
    public string Stem { get; set; } = "Default"; // 题目文本
    public double Weight { get; set; } = 1.0; // 分数

    public string[]? UserAnswer; // 用户答案
    public string[] Answer; // 标准答案
    public string[]? ReferenceAnswer; // 参考答案
    
    public string[]? ReferenceMaterials { get; set; } = new string[] { }; // 参考资料
    
    public bool IsAiJudge { get; set; } = false; // 是否需要AI判题
    public string[]? Commits { get; set; } = new string[] { }; // AI提示

    
    
    
    
    #region ISerializable

    public Question() {}
    
    protected Question(SerializationInfo info, StreamingContext context)
    {
        QuestionId = info.GetString("QuestionId");
        Type = (QuestionTypes) info.GetInt32("Type");
        Stem = info.GetString("Stem")!;
        Weight = info.GetDouble("Weight");
        
        UserAnswer = (string[]?) info.GetValue("UserAnswer", typeof(string[]));
        Answer = (string[]) info.GetValue("Answer", typeof(string[]))!;
        ReferenceAnswer = (string[]?) info.GetValue("ReferenceAnswer", typeof(string[]));
        
        ReferenceMaterials = (string[]?) info.GetValue("ReferenceMaterials", typeof(string[]));
        
        IsAiJudge = info.GetBoolean("IsAiJudge");
        Commits = (string[]?) info.GetValue("Commits", typeof(string[]));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("QuestionId", QuestionId);
        info.AddValue("Type", (int) Type);
        info.AddValue("Stem", Stem);
        info.AddValue("Weight", Weight);
        
        info.AddValue("UserAnswer", UserAnswer);
        info.AddValue("Answer", Answer);
        info.AddValue("ReferenceAnswer", ReferenceAnswer);
        
        info.AddValue("ReferenceMaterials", ReferenceMaterials);
        
        info.AddValue("IsAiJudge", IsAiJudge);
        info.AddValue("Commits", Commits);
    }

    #endregion
    
}