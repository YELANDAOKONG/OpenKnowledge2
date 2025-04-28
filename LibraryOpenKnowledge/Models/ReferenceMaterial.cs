using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterial : ISerializable
{
    
    public string[] Materials { get; set; } = new string[] { };
    public ReferenceMaterialImage[]? Images { get; set; } = null;



    #region ISerializable

    public ReferenceMaterial() { }

    protected ReferenceMaterial(SerializationInfo info, StreamingContext context)
    {
        Materials = (string[])info.GetValue("Materials", typeof(string[]))!;
        Images = (ReferenceMaterialImage[])info.GetValue("Images", typeof(ReferenceMaterialImage[]))!;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Materials", Materials);
        info.AddValue("Images", Images);
    }

    #endregion
    
}