using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public enum ReferenceMaterialImageTypes
{
    Unknown = 0,
    Local = 1,
    Remote = 2,
    Embedded = 3,
    Packaged = 4,
}