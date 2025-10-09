using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Links.Services;

namespace Sunflower.Links;

/// <summary>
/// PIF binary is a "Program Information File"
/// contained link to existed .COM/.EXE file in old
/// DOS|Windows|OS/2 operating systems.
/// </summary>
[FlowerSeedContract(3, 0, 0)]
public class PifFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower MS-DOS PIF Viewer";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            PifDumpManager dumpManager = new(path);
            PifTableManager tableManager = new(dumpManager);

            List<string> intoList =
            [
                "### File information",
                $" - FileName: {path}",
                $" - Size: {new FileInfo(path).Length / 1024}K",
                $" - Created at {new FileInfo(path).CreationTimeUtc}"
            ];

            var intro = new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = intoList,
            };
            // define Program information start by file extension or first WORD
            // see "Microsoft PIF structure.pdf" in git repo.
            Status.Results.Add(intro);
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.DataTables)
            {
                BoxedResult = new List<DataTable>()
                {
                    tableManager.SectionHeaders,
                    tableManager.MicrosoftPifExTable,
                    tableManager.Windows386Table,
                    tableManager.Windows286Table,
                }
            });

            Status.IsEnabled = true;
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}