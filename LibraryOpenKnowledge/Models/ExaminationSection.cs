using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ExaminationSection : ISerializable
{
    
    
    

    #region ISerializable

    public ExaminationSection() { }

    protected ExaminationSection(SerializationInfo info, StreamingContext context)
    {
        
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        
    }

    #endregion
    
}