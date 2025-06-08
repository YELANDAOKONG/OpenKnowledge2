using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialDocument
{
    public ReferenceMaterialDocumentTypes Type { get; set; } = ReferenceMaterialDocumentTypes.Unknown;
    public string? Uri { get; set; } = null;
    public byte[]? Document { get; set; } = null;

}