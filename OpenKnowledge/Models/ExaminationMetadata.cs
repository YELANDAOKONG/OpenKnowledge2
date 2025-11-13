using System.Runtime.Serialization;

namespace OpenKnowledge.Models;


[Serializable]
public class ExaminationMetadata
{
    public string? ExamId { get; set; } = null; // 考试ID
    public string Title { get; set; } = "Default"; // 考试标题
    public string? Description { get; set; } = null; // 考试描述

    public string? Subject { get; set; } = null; // 试卷学科
    public string? Language { get; set; } = null; // 试卷语言
    public double TotalScore { get; set; } = 0;
    
    public long? ExamTime { get; set; } = null; // 推荐考试时间
    public long? MinimumExamTime { get; set; } = null; // 最小考试时间（考试开始时间为 0 计时的毫秒时间戳，超过此时间后可提前胶卷）
    public long? MaximumExamTime { get; set; } = null; // 最大考试时间（考试开始时间为 0 计时的毫秒时间戳，超过此时间后无法继续考试）

    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = []; // 试卷参考资料
}