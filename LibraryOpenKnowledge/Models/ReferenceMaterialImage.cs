using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialImage
{
    public ReferenceMaterialImageTypes Type { get; set; } = ReferenceMaterialImageTypes.Unknown;
    
    public string? Format { get; set; } = null;
    public string? Uri { get; set; } = null;
    public byte[]? Image { get; set; } = null;

}