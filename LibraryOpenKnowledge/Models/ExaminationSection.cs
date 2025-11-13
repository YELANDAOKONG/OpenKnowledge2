using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ExaminationSection
{
    public string? SectionId { get; set; } = null; // 章节ID
    public string Title { get; set; } = "Default"; // 章节标题
    public string? Description { get; set; } = null; // 章节描述
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = []; // 章节参考材料
    
    public double? Score { get; set; } = null; // 章节分数
    public Question[]? Questions { get; set; } = [];
}