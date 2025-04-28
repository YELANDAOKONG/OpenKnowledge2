using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ReferenceMaterial
{
    public string[] Materials { get; set; } = new string[] { };
    public ReferenceMaterialImage[]? Images { get; set; } = null;
}