using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class Examination : ISerializable
{
    public ExaminationVersion ExaminationVersion { get; set; } = new();
    public ExaminationMetadata ExaminationMetadata { get; set; } = new();
    public ExaminationSection[] ExaminationSections { get; set; } = new ExaminationSection[] { };
    

    #region ISerializable
    
    public Examination() { }
    
    protected Examination(SerializationInfo info, StreamingContext context)
    {
        ExaminationVersion = (ExaminationVersion) info.GetValue("ExaminationVersion", typeof(ExaminationVersion))!;
        ExaminationMetadata = (ExaminationMetadata) info.GetValue("ExaminationMetadata", typeof(ExaminationMetadata))!;
        ExaminationSections = (ExaminationSection[]) info.GetValue("ExaminationSections", typeof(ExaminationSection[]))!;
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("ExaminationVersion", ExaminationVersion);
        info.AddValue("ExaminationMetadata", ExaminationMetadata);
        info.AddValue("ExaminationSections", ExaminationSections);
    }
    
    #endregion
}