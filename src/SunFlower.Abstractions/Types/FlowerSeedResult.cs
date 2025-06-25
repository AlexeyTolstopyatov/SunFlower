namespace SunFlower.Abstractions.Types;

public class FlowerSeedResult
{
    public FlowerSeedEntryType Type { get; set; }
    public object BoxedResult { get; set; } = 0; // always not null
}