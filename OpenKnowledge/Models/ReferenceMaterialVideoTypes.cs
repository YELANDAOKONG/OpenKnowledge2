using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public enum ReferenceMaterialVideoTypes
{
    Unknown = 0,
    Local = 1,
    Remote = 2,
    Embedded = 3,
    Packaged = 4,
}