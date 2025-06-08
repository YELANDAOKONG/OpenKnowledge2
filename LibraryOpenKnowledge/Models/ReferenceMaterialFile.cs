using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialFile
{
    public ReferenceMaterialFileTypes Type { get; set; } = ReferenceMaterialFileTypes.Unknown;
    public string? Uri { get; set; } = null;
    public byte[]? Document { get; set; } = null;

}