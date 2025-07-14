using SunFlower.Abstractions;

namespace Sunflower.Links;

/// <summary>
/// PIF binary is a "Program Information File"
/// contained link to existed .COM/.EXE file in old
/// DOS|Windows|OS/2 operating systems.
/// </summary>
public class PifFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower MS-DOS PIF Viewer";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            // define Program information start by file extension or first WORD
            // see "Microsoft PIF structure.pdf" in git repo.
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}