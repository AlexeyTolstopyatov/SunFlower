using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Services;

public class NeStructVisualizer(NeHeader @struct) : AbstractStructVisualizer<NeHeader>(@struct)
{
    public override DataTable ToDataTable()
    {
        var ne = _struct;
        DataTable table = new()
        {
            TableName = "Windows-OS/2 New Executable"
        };
        
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);
        table.Rows.Add(nameof(ne.NE_ID), "0x" + ne.NE_ID.ToString("X"));
        table.Rows.Add(nameof(ne.NE_LinkerVersion), "0x" + ne.NE_LinkerVersion.ToString("X"));
        table.Rows.Add(nameof(ne.NE_LinkerRevision), "0x" + ne.NE_LinkerRevision.ToString("X"));
        table.Rows.Add(nameof(ne.NE_EntryTable), "0x" + ne.NE_EntryTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_EntriesCount), "0x" + ne.NE_EntriesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Checksum), "0x" + ne.NE_Checksum.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Flags), "0x" + ne.NE_Flags.ToString("X"));
        table.Rows.Add(nameof(ne.NE_AutoSegment), "0x" + ne.NE_AutoSegment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Heap), "0x" + ne.NE_Heap.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Stack), "0x" + ne.NE_Stack.ToString("X"));
        table.Rows.Add(nameof(ne.NE_CsIp), "0x" + ne.NE_CsIp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SsSp), "0x" + ne.NE_SsSp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SegmentsCount), "0x" + ne.NE_SegmentsCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ModReferencesCount), "0x" + ne.NE_ModReferencesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_NonResidentNamesCount), $"0x{ne.NE_NonResidentNamesCount:X}");
        table.Rows.Add(nameof(ne.NE_SegmentsTable), "0x" + ne.NE_SegmentsTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResourcesTable), "0x" + ne.NE_ResourcesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResidentNamesTable), "0x" + ne.NE_ResidentNamesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ModReferencesTable), $"0x{ne.NE_ModReferencesTable:X}");
        table.Rows.Add(nameof(ne.NE_ImportModulesTable), $"0x{ne.NE_ImportModulesTable:X}");
        table.Rows.Add(nameof(ne.NE_NonResidentNamesTable), $"0x{ne.NE_NonResidentNamesTable:X}");
        table.Rows.Add(nameof(ne.NE_MovableEntriesCount), "0x" + ne.NE_MovableEntriesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Alignment), "0x" + ne.NE_Alignment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResourcesCount), $"0x{ne.NE_ResourcesCount:X}");
        table.Rows.Add(nameof(ne.NE_OS), "0x" + ne.NE_OS.ToString("X"));
        table.Rows.Add(nameof(ne.NE_FlagOthers), "0x" + ne.NE_FlagOthers.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PretThunks), "0x" + ne.NE_PretThunks.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PerSegmentRefByte), "0x" + ne.NE_PerSegmentRefByte.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SwapArea), "0x" + ne.NE_SwapArea.ToString("X"));
        table.Rows.Add(nameof(ne.NE_WindowsVersionMinor), "0x" + ne.NE_WindowsVersionMinor.ToString("X"));
        table.Rows.Add(nameof(ne.NE_WindowsVersionMajor), "0x" + ne.NE_WindowsVersionMajor.ToString("X"));

        return table;
    }

    public override string ToString()
    {
        return new FlowerDescriptor()
            .Line("The New executable header is a necessary part of ")
            .Line("every Windows or OS/2 **segmented** executable.")
            .Line("This model of executable image allows to use external resources at runtime,")
            .Line("running in x86 protected mode.")
            .Line("All files marked as `NE` are 16-bit binaries and addresses of all procedures")
            .Line("are 16-bit too.")
            .ToString();
    }

    public override Region ToRegion()
    {
        return new Region("## Windows-OS/2 Executable Header", ToString(), ToDataTable());
    }
}