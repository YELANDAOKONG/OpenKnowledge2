using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class Examination
{
    public ExaminationVersion ExaminationVersion { get; set; } = new();
    public ExaminationMetadata ExaminationMetadata { get; set; } = new();
    public ExaminationSection[] ExaminationSections { get; set; } = [];
    public ExaminationExtension[] ExaminationExtensions { get; set; } = [];
}