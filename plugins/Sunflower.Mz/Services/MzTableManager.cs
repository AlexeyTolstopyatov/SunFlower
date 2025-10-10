using SunFlower.Abstractions.Types;

namespace Sunflower.Mz.Services;

public class MzTableManager
{
    public List<Region> Regions { get; } = [];
    
    public MzTableManager(MzDumpManager dumpManager)
    {
        Regions.Add(new MzStructVisualizer(dumpManager.Header).ToRegion());
        
        if (dumpManager.Header.e_relc > 0)
            Regions.Add(new MzRelocationsVisualizer(dumpManager.Relocations).ToRegion());
        
        Regions.Add(new MzDetailsVisualizer(dumpManager.Header).ToRegion());
    }
}