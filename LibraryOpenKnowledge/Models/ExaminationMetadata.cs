using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;


[Serializable]
public class ExaminationMetadata : ISerializable
{
    
    public string? ExamId { get; set; } = null; // 考试ID
    public string Title { get; set; } = "Default"; // 考试标题
    public string? Description { get; set; } = null; // 考试描述
    
    public string? Subject { get; set; } = null; // 试卷学科
    public string? Language { get; set; } = null; // 试卷语言
    
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { }; // 试卷参考资料
    
    
    #region ISerializable
    
    public ExaminationMetadata() { }

    protected ExaminationMetadata(SerializationInfo info, StreamingContext context)
    {
        ExamId = info.GetString("ExamId");
        Title = info.GetString("Title") ?? "Default";
        Description = info.GetString("Description");
        
        Subject = info.GetString("Subject");
        Language = info.GetString("Language");
        ReferenceMaterials = (ReferenceMaterial[]?) info.GetValue("ReferenceMaterials", typeof(ReferenceMaterial[]));
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("ExamId", ExamId);
        info.AddValue("Title", Title);
        info.AddValue("Description", Description);
        
        info.AddValue("Subject", Subject);
        info.AddValue("Language", Language);
        info.AddValue("ReferenceMaterials", ReferenceMaterials);
    }
    
    #endregion
}