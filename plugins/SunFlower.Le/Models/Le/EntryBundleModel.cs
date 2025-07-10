using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models.Le;

public class EntryBundleModel(EntryBundle bundle, List<string> flags)
{
    public EntryBundle EntryBundle { get; set; } = bundle;
    public string[] Flags { get; set; } = flags.ToArray();
}