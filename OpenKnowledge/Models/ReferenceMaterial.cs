using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class ReferenceMaterial
{
    public string[] Materials { get; set; } = [];
    
    public ReferenceMaterialDocument[]? Documents { get; set; } = null;
    
    public ReferenceMaterialImage[]? Images { get; set; } = null;
    public ReferenceMaterialAudio[]? Audios { get; set; } = null;
    public ReferenceMaterialVideo[]? Videos { get; set; } = null;
    
    public ReferenceMaterialFile[]? Files { get; set; } = null;
    
}