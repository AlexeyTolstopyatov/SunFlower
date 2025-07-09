using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models;

public class ObjectTableModel(ObjectTable table, List<string> flags)
{
    public ObjectTable ObjectTable { get; set; } = table;
    public string[] ObjectFlags { get; set; } = flags.ToArray();
}