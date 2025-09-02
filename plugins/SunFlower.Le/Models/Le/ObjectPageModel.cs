using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models.Le;

public class ObjectPageModel(ObjectPage page, List<string> flags)
{
    public ObjectPage Page { get; set; } = page;
    public string[] Flags { get; set; } = flags.ToArray();
}