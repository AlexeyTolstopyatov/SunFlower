using Object = SunFlower.Le.Headers.Le.Object;

namespace SunFlower.Le.Models.Le;

public class ObjectTableModel(Object table, List<string> flags)
{
    public Object Object { get; set; } = table;
    public string[] ObjectFlags { get; set; } = flags.ToArray();
}