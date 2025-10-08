using SunFlower.Abstractions;
using SunFlower.Pe.Headers;
using SunFlower.Pe.Services;

namespace SunFlower.Pe;

[FlowerSeedContract(3, 0, 0)]
public class VisualBasicFlowerSeed : IFlowerSeed
{
    public string Seed => "Sunflower VisualBasic Runtime";
    public FlowerSeedStatus Status { get; } = new();
    
    public int Main(string path)
    {
        try
        {
            var peManager = new PeDumpManager(path);

            // null! tells that struct by-value initialized already
            // but the Magic signature not. Magic.Length will throw NullReferenceException.
            
            var isVb5Defined = peManager.Vb5Header.VbMagic != null!;
            var isVb4Defined = peManager.Vb4Header.Signature != null!;

            if (isVb5Defined)
                FindVb5Details(peManager.VbOffset, peManager.Vb5Header);
        }
        catch (Exception e)
        {
            Status.LastError = e;
        }

        return -1;
    }

    private static FlowerSeedStatus FindVb5Details(long offset, Vb5Header header)
    {
        var status = new FlowerSeedStatus();
        
        
        
        return status;
    }
}