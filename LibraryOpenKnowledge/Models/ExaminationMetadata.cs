using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;


[Serializable]
public class ExaminationMetadata
{
    public string? ExamId { get; set; } = null; // 考试ID
    public string Title { get; set; } = "Default"; // 考试标题
    public string? Description { get; set; } = null; // 考试描述

    public string? Subject { get; set; } = null; // 试卷学科
    public string? Language { get; set; } = null; // 试卷语言
    public double TotalScore { get; set; } = 0;

    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { }; // 试卷参考资料
}