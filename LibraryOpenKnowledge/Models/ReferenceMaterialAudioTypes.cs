using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public enum ReferenceMaterialAudioTypes
{
    Unknown = 0,
    Local = 1,
    Remote = 2,
    Embedded = 3,
    Packaged = 4,
}