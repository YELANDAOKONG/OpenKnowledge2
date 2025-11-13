namespace OpenKnowledge.Models;

[Serializable]
public class ExaminationExtension
{
    public string Id { get; set; } = String.Empty;
    
    public Dictionary<string, string?> Extensions { get; set; } = new(); 
}