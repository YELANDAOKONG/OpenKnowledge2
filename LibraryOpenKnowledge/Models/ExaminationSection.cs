using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ExaminationSection : ISerializable
{
    
    public string? SectionId { get; set; } = null; // 章节ID
    public string Title { get; set; } = "Default"; // 章节标题
    public string? Description { get; set; } = null; // 章节描述
    public ReferenceMaterial[]? ReferenceMaterials { get; set; } = new ReferenceMaterial[] { }; // 章节参考材料
    
    

    #region ISerializable

    public ExaminationSection() { }

    protected ExaminationSection(SerializationInfo info, StreamingContext context)
    {
        SectionId = info.GetString("SectionId");
        Title = info.GetString("Title") ?? "Default";
        Description = info.GetString("Description");
        ReferenceMaterials = (ReferenceMaterial[]?) info.GetValue("ReferenceMaterials", typeof(ReferenceMaterial[]));
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        
    }

    #endregion
    
}