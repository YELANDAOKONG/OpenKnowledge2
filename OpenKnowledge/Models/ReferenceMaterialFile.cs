using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialFile
{
    public ReferenceMaterialFileTypes Type { get; set; } = ReferenceMaterialFileTypes.Unknown;
    
    public string? Format { get; set; } = null;
    public string? Uri { get; set; } = null;
    public byte[]? Document { get; set; } = null;

}