using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class ReferenceMaterialAudio
{
    public ReferenceMaterialAudioTypes Type { get; set; } = ReferenceMaterialAudioTypes.Unknown;
    
    public string? Format { get; set; } = null;
    public string? Uri { get; set; } = null;
    public byte[]? Audio { get; set; } = null;

}