using System.Data;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeTableManager
{
    private readonly NeDumpManager _manager;
    public NeTableManager(NeDumpManager manager)
    {
        _manager = manager;
        MakeCharacteristics();
        
        MakeHeadersTables();
        MakeSegmentsTable();
        MakeEntryPointsTable();
        MakeModuleReferencesTable();
        MakeNames();
    }
    
    public DataTable[] Headers { get; set; } = [];
    public DataTable SegmentTable { get; set; } = new();
    public DataTable ResidentNamesTable { get; set; } = new();
    public DataTable NamesTable { get; set; } = new();
    public DataTable EntryPointsTable { get; set; } = new();
    public DataTable ModuleReferencesTable { get; set; } = new();
    // public DataTable ImportingNamesTable { get; set; } = new();
    public string[] Characteristics { get; set; } = [];
    
    private void MakeHeadersTables()
    {
        DataTable dosHeader = MakeDosHeader();
        DataTable windowsHeader = MakeWindowsHeader();
                
        Headers = [dosHeader, windowsHeader];
    }
    
    private DataTable MakeDosHeader()
    {
        MzHeader mz = _manager.MzHeader;
        DataTable table = new()
        {
            TableName = "DOS/2 Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.e_sign), mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_lastb), mz.e_lastb.ToString("X"));
        table.Rows.Add(nameof(mz.e_fbl), mz.e_fbl.ToString("X"));
        table.Rows.Add(nameof(mz.e_relc), mz.e_relc.ToString("X"));
        table.Rows.Add(nameof(mz.e_pars), mz.e_pars.ToString("X"));
        table.Rows.Add(nameof(mz.e_minep), mz.e_minep.ToString("X"));
        table.Rows.Add(nameof(mz.e_maxep), mz.e_maxep.ToString("X"));
        table.Rows.Add(nameof(mz.ss), mz.ss.ToString("X"));
        table.Rows.Add(nameof(mz.sp), mz.sp.ToString("X"));
        table.Rows.Add(nameof(mz.e_check), mz.e_check.ToString("X"));
        table.Rows.Add(nameof(mz.ip), mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_reltableoff), mz.e_reltableoff.ToString("X"));
        table.Rows.Add(nameof(mz.e_overnum), mz.e_overnum.ToString("X"));
        table.Rows.Add(nameof(mz.e_oemid), mz.e_oemid.ToString("X"));
        table.Rows.Add(nameof(mz.e_oeminfo), mz.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfanew), mz.e_lfanew.ToString("X"));
        
        return table;
    }
    
    private DataTable MakeWindowsHeader()
    {
        NeHeader ne = _manager.NeHeader;
        DataTable table = new()
        {
            TableName = "Windows-OS/2 New Executable"
        };
        
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);
        table.Rows.Add(nameof(ne.NE_ID), ne.NE_ID.ToString("X"));
        table.Rows.Add(nameof(ne.NE_LinkerVersion), ne.NE_LinkerVersion.ToString("X"));
        table.Rows.Add(nameof(ne.NE_LinkerRevision), ne.NE_LinkerRevision.ToString("X"));
        table.Rows.Add(nameof(ne.NE_EntryTable), ne.NE_EntryTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_EntriesCount), ne.NE_EntriesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Checksum), ne.NE_Checksum.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ProgramFlags), ne.NE_ProgramFlags.ToString("X"));
        table.Rows.Add(nameof(ne.NE_AppFlags), ne.NE_AppFlags.ToString("X"));
        table.Rows.Add(nameof(ne.NE_AutoSegment), ne.NE_AutoSegment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Heap), ne.NE_Heap.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Stack), ne.NE_Stack.ToString("X"));
        table.Rows.Add(nameof(ne.NE_CsIp), ne.NE_CsIp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SsSp), ne.NE_SsSp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SegmentsCount), ne.NE_SegmentsCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ModReferencesCount), ne.NE_ModReferencesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SegmentsTable), ne.NE_SegmentsTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResourcesTable), ne.NE_ResourcesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResidentNamesTable), ne.NE_ResidentNamesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_MovableEntriesCount), ne.NE_MovableEntriesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Alignment), ne.NE_Alignment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_OS), ne.NE_OS.ToString("X"));
        table.Rows.Add(nameof(ne.NE_FlagOthers), ne.NE_FlagOthers.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PretThunks), ne.NE_PretThunks.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PerSegmentRefBytes), ne.NE_PerSegmentRefBytes.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SwapArea), ne.NE_SwapArea.ToString("X"));
        table.Rows.Add(nameof(ne.NE_WindowsVersionMinor), ne.NE_WindowsVersionMinor.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ID), ne.NE_WindowsVersionMajor.ToString("X"));

        return table;
    }

    private void MakeSegmentsTable()
    {
        if (_manager.SegmentModels.Count == 0)
            return;
        
        DataTable segs = new("Segments Table");
        
        segs.Columns.AddRange([
            new DataColumn("Segmentation Type"), 
            new DataColumn("#Segment"), 
            new DataColumn("Relative Offset (NVA)"),
            new DataColumn("Segment Length"),
            new DataColumn("Segment Characteristics"),
            new DataColumn("Minimum Allocation"),
            new DataColumn("Characteristics")
        ]);

        foreach (NeSegmentModel segmentDump in _manager.SegmentModels)
        {
            string array = segmentDump
                .Characteristics
                .Aggregate("", (current, characteristic) => current + (characteristic + " "));
            
            segs.Rows.Add(
                segmentDump.Type,
                segmentDump.SegmentNumber.ToString("X"),
                segmentDump.FileOffset.ToString("X"),
                segmentDump.FileLength.ToString("X"),
                segmentDump.Flags.ToString("X"),
                segmentDump.MinAllocation.ToString("X"),
                array
            );
        }

        SegmentTable = segs;
    }

    private void MakeEntryPointsTable()
    {
        if (_manager.EntryTableItems.Count == 0)
            return;
        DataTable entries = new("EntryTable")
        {
            Columns = {"Ordinal", "Offset", "Segment", "Entry", "Data type", "Entry type" }
        };
        
        foreach (NeEntryTableModel item in _manager.EntryTableItems)
        {
            entries.Rows.Add(
                "@" + item.Ordinal,
                item.Offset.ToString("X"),
                item.Segment,
                item.Entry,
                item.Data,
                item.Type
            );
        }

        EntryPointsTable = entries;
    }

    private void MakeModuleReferencesTable()
    {
        if (_manager.ModuleReferences.Length == 0)
            return;
        
        DataTable modres = new("Module References")
        {
            Columns = { "Reference#", "Reference Offset" }
        };
        for (int i = 0; i < _manager.ModuleReferences.Length; ++i)
        {
            modres.Rows.Add(
                i + 1,
                _manager.ModuleReferences[i].ImportOffset
            );
        }

        ModuleReferencesTable = modres;
    }

    private void MakeNames()
    {
        if (_manager.NonResidentNames.Length == 0)
            return;
        
        DataTable nonres = new("NonResident/Resident Names table")
        {
            Columns = { "Count", "Name", "Ordinal", "Name Table" }
        };
        foreach (NeExport neExportDump in _manager.NonResidentNames)
        {
            nonres.Rows.Add(
                neExportDump.Count,
                neExportDump.Name,
                "@" + neExportDump.Ordinal,
                "[Not resident]"
            );
        }

        foreach (NeExport export in _manager.ResidentNames)
        {
            nonres.Rows.Add(
                export.Count,
                export.Name,
                "@" + export.Ordinal,
                "[Resident]"
            );
        }

        NamesTable = nonres;
    }

    private void MakeCharacteristics()
    {
        List<string> md = [];
        md.Add("### Image");

        string os = _manager.NeHeader.NE_OS switch
        {
            0x1 => "OS/2",
            0x2 => "Win16",
            0x3 => "DOS/4",
            0x4 => "Win32s",
            0x5 => "BoSS",
            _ => "Not specified"
        };

        string cpu = _manager.NeHeader.NE_ProgramFlags switch
        {
            var f when (f & 0x4) != 0 => "I8086",
            var f when (f & 0x5) != 0 => "I286",
            var f when (f & 0x6) != 0 => "I386",
            var f when (f & 0x7) != 0 => "I8087",
            _ => "Not specified"
        };
        md.Add($"Target operating system environment: `{os}`");
        md.Add($"Target CPU architecture: `{cpu}`");

        md.Add("### Importing modules");
        md.Add("All .DLL/EXE module names which resolved successfully");

        foreach (string mod in _manager.ImportModels.Select(m => m.DllName))
        {
            md.Add($" - `{mod}`");
        }
        
        Characteristics = md.ToArray();
    }
}