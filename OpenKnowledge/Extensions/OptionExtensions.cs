using OpenKnowledge.Models;

namespace OpenKnowledge.Extensions;

public static class OptionExtensions
{
    public static List<Option> ToOptionList(this List<(string, string)>? tuples)
    {
        if (tuples == null) return new List<Option>();
        return tuples.Select(t => new Option(t.Item1, t.Item2)).ToList();
    }
    
    public static List<(string, string)> ToTupleList(this List<Option>? options)
    {
        if (options == null) return new List<(string, string)>();
        return options.Select(o => (o.Id, o.Text)).ToList();
    }
}
