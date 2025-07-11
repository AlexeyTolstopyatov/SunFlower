using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models.Le;

public class EntryBundleModel(int num, EntryBundle bundle, List<string> flags)
{
    public int BundleNumber { get; set; } = num;
    public EntryBundle EntryBundle { get; set; } = bundle;
    public string[] Flags { get; set; } = flags.ToArray();
}