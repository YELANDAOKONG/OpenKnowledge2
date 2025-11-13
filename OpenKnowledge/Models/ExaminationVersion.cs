using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class ExaminationVersion : ISerializable
{

    public int Major { get; set; } = 0;
    public int Minor { get; set; } = 0;
    public int Patch { get; set; } = 0;
    
    #region ISerializable
    
    public ExaminationVersion() { }
    
    public ExaminationVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    protected ExaminationVersion(SerializationInfo info, StreamingContext context)
    {
        Major = info.GetInt32("Major");
        Minor = info.GetInt32("Minor");
        Patch = info.GetInt32("Patch");
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Major", Major);
        info.AddValue("Minor", Minor);
        info.AddValue("Patch", Patch);
    }
    
    #endregion
}