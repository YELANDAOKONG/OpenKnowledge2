namespace LibraryOpenKnowledge.Models;

[Serializable]
public class Option
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public string Item1 => Id;
    public string Item2 => Text;
    
    public Option() { }
    
    public Option(string id, string text)
    {
        Id = id;
        Text = text;
    }
    
    public static implicit operator (string, string)(Option option)
    {
        return (option.Id, option.Text);
    }
    
    public static implicit operator Option((string id, string text) tuple)
    {
        return new Option(tuple.id, tuple.text);
    }
}