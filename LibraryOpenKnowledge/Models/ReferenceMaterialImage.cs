using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialImage : ISerializable
{
    
    public ReferenceMaterialImageTypes Type { get; set; } = ReferenceMaterialImageTypes.Unknown;
    public string? Uri { get; set; } = null;
    public byte[]? Image { get; set; } = null;

    
    
    
    #region ISerializable

    public ReferenceMaterialImage() { }
    
    protected ReferenceMaterialImage(SerializationInfo info, StreamingContext context)
    {
        Type = (ReferenceMaterialImageTypes)info.GetInt32("Type");
        Uri = info.GetString("Uri");
        Image = info.GetValue("Image", typeof(byte[])) as byte[];
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Type", (int)Type);
        info.AddValue("Uri", Uri);
        info.AddValue("Image", Image);
    }

    #endregion
    
    
}