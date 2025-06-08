using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialVideo
{
    public ReferenceMaterialVideoTypes Type { get; set; } = ReferenceMaterialVideoTypes.Unknown;
    
    public string? Format { get; set; } = null;
    public string? Uri { get; set; } = null;
    public byte[]? Video { get; set; } = null;

}