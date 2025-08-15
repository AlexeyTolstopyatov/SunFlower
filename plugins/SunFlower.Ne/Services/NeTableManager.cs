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
        MakeSegmentRelocations();
        MakeEntryPointsTable();
        MakeModuleReferencesTable();
        MakeNames();
        MakeImports();
    }
    
    public DataTable[] Headers { get; set; } = [];
    public DataTable SegmentTable { get; set; } = new("Table of file Segments");
    public DataTable[] RelocationsTables { get; set; } = [];
    public DataTable NamesTable { get; set; } = new("Resident/NonResident Names");
    public DataTable[] EntryTables { get; set; } = [];
    public DataTable ModuleReferencesTable { get; set; } = new("Module References table");
    // public DataTable ImportingNamesTable { get; set; } = new();
    public string[] Characteristics { get; set; } = [];
    public string[] Imports { get; set; } = [];

    private void MakeHeadersTables()
    {
        var dosHeader = MakeDosHeader();
        var windowsHeader = MakeWindowsHeader();
                
        Headers = [dosHeader, windowsHeader];
    }
    
    private DataTable MakeDosHeader()
    {
        var mz = _manager.MzHeader;
        DataTable table = new()
        {
            TableName = "DOS/2 Extended Executable"
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
        var ne = _manager.NeHeader;
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
        table.Rows.Add(nameof(ne.NE_ProgramFlags), "0x" + ne.NE_ProgramFlags.ToString("X"));
        table.Rows.Add(nameof(ne.NE_AppFlags), "0x" + ne.NE_AppFlags.ToString("X"));
        table.Rows.Add(nameof(ne.NE_AutoSegment), "0x" + ne.NE_AutoSegment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Heap), "0x" + ne.NE_Heap.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Stack), "0x" + ne.NE_Stack.ToString("X"));
        table.Rows.Add(nameof(ne.NE_CsIp), "0x" + ne.NE_CsIp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SsSp), "0x" + ne.NE_SsSp.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SegmentsCount), "0x" + ne.NE_SegmentsCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ModReferencesCount), "0x" + ne.NE_ModReferencesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SegmentsTable), "0x" + ne.NE_SegmentsTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResourcesTable), "0x" + ne.NE_ResourcesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_ResidentNamesTable), "0x" + ne.NE_ResidentNamesTable.ToString("X"));
        table.Rows.Add(nameof(ne.NE_MovableEntriesCount), "0x" + ne.NE_MovableEntriesCount.ToString("X"));
        table.Rows.Add(nameof(ne.NE_Alignment), "0x" + ne.NE_Alignment.ToString("X"));
        table.Rows.Add(nameof(ne.NE_OS), "0x" + ne.NE_OS.ToString("X"));
        table.Rows.Add(nameof(ne.NE_FlagOthers), "0x" + ne.NE_FlagOthers.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PretThunks), "0x" + ne.NE_PretThunks.ToString("X"));
        table.Rows.Add(nameof(ne.NE_PerSegmentRefBytes), "0x" + ne.NE_PerSegmentRefBytes.ToString("X"));
        table.Rows.Add(nameof(ne.NE_SwapArea), "0x" + ne.NE_SwapArea.ToString("X"));
        table.Rows.Add(nameof(ne.NE_WindowsVersionMinor), "0x" + ne.NE_WindowsVersionMinor.ToString("X"));
        table.Rows.Add(nameof(ne.NE_WindowsVersionMajor), "0x" + ne.NE_WindowsVersionMajor.ToString("X"));

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

        foreach (var segmentDump in _manager.SegmentModels)
        {
            var array = segmentDump
                .Characteristics
                .Aggregate("", (current, characteristic) => current + (characteristic + " "));
            
            segs.Rows.Add(
                segmentDump.Type,
                "0x" + segmentDump.SegmentNumber.ToString("X"),
                "0x" + segmentDump.FileOffset.ToString("X"),
                "0x" + segmentDump.FileLength.ToString("X"),
                "0x" + segmentDump.Flags.ToString("X"),
                "0x" + segmentDump.MinAllocation.ToString("X"),
                array
            );
        }

        SegmentTable = segs;
    }

    private void MakeEntryPointsTable()
    {
        if (_manager.EntryTableItems.Count == 0)
            return;
        
        var counter = 1;
        var tables = new List<DataTable>();
        
        foreach (NeEntryBundle bundle in _manager.EntryTableItems)
        {
            DataTable entries = new($"EntryTable Bundle #{counter}")
            {
                Columns = {"Ordinal", "Offset", "Segment", "Entry", "Data type", "Entry type" }
            };
            foreach (var item in bundle.EntryPoints)
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

            counter++;
            tables.Add(entries);
        }

        EntryTables = tables.ToArray();
    }

    private void MakeModuleReferencesTable()
    {
        if (_manager.ModuleReferences.Length == 0)
            return;
        
        DataTable modres = new("Module References")
        {
            Columns = { "Reference#", "Reference Offset" }
        };
        for (var i = 0; i < _manager.ModuleReferences.Length; ++i)
        {
            modres.Rows.Add(
                i + 1,
                _manager.ModuleReferences[i].ImportOffset
            );
        }

        ModuleReferencesTable = modres;
    }

    private void MakeSegmentRelocations()
    {
        var tables = new List<DataTable>();
        var sexWithRelocations = _manager.SegmentModels.Where(s => s.Relocations.Count > 0).Distinct().ToList(); 
        
        foreach (var segment in sexWithRelocations)
        {
            var tableName = $"Relocations Table for Segment #{segment.SegmentNumber} ({segment.Type})";
            DataTable table = new(tableName)
            {
                Columns =
                {
                    "ATP", 
                    "RTP",
                    "RTP String",
                    "Is Additive",
                    "OffsetInSeg",
                    "SegType",
                    "Target",
                    "Target Type",
                    "Mod#",
                    "Name",
                    "Ordinal",
                    "Fixup"
                }
            };

            foreach (var rel in segment.Relocations)
            {
                // prepare table
                table.Rows.Add(
                    rel.AddressType, 
                    rel.RelocationFlags, 
                    rel.RelocationType, 
                    rel.IsAdditive,
                    rel.OffsetInSegment, 
                    rel.SegmentType, 
                    rel.Target, 
                    rel.TargetType, 
                    rel.ModuleIndex,
                    rel.Ordinal, 
                    rel.NameOffset);
            }
            tables.Add(table);
        }

        RelocationsTables = tables.ToArray();
    }
    private void MakeNames()
    {
        if (_manager.NonResidentNames.Length == 0)
            return;
        
        DataTable nonres = new("NonResident/Resident Names table")
        {
            Columns = { "Count", "Name", "Ordinal", "Name Table" }
        };
        foreach (var neExportDump in _manager.NonResidentNames)
        {
            nonres.Rows.Add(
                neExportDump.Count,
                OnlyAscii(neExportDump.Name),
                "@" + neExportDump.Ordinal,
                "[Not resident]"
            );
        }

        foreach (var export in _manager.ResidentNames)
        {
            nonres.Rows.Add(
                export.Count,
                OnlyAscii(export.Name),
                "@" + export.Ordinal,
                "[Resident]"
            );
        }

        NamesTable = nonres;
    }

    private void MakeImports()
    {
        List<string> md = [];
        
        md.Add("\r\n### Importing modules");
        md.Add("All .DLL/.EXE module names which resolved successfully");
        
        //md.AddRange(_manager.ImportModels.Select(m => m.DllName).Select(mod => $" - `{mod}`"));

        foreach (var importModel in _manager.ImportModels)
        {
            md.Add($" - `{importModel.DllName}`");
            foreach (var function in importModel.Functions)
            {
                md.Add($"    - `{function.Name}@{function.Ordinal}`");
            }
        }

        Imports = md.ToArray();
    }
    private void MakeCharacteristics()
    {
        List<string> md = [];
        
        //md.Add("_Main information details took from Windows New segmented EXE header (called `IMAGE_OS2_HEADER` in Win32 API)_\r\n");
        md.Add("\r\n### Image");
        md.Add($"Project Name: `{OnlyAscii(_manager.ResidentNames[0].Name)}`"); // <-- first name always project-name
        md.Add($"Description: {OnlyAscii(_manager.NonResidentNames[0].Name)}");
        
        var os = _manager.NeHeader.NE_OS switch
        {
            0x0 => "Any OS supported", // set for *.FON. means "any OS supported"  
            0x1 => "OS/2 (Os2ss)",
            0x2 => "Windows/286 (Win16)",
            0x3 => "DOS/4",
            0x4 => "Windows/386 (Win16)",
            0x5 => "BoSS",
            _ => $"Unknown 0x{_manager.NeHeader.NE_OS:X}" // <-- really don't know how handle it
        };

        var cpu = _manager.NeHeader.NE_ProgramFlags switch
        {
            var f when (f & 0x4) != 0 => "I8086",
            var f when (f & 0x5) != 0 => "I286",
            var f when (f & 0x6) != 0 => "I386",
            var f when (f & 0x7) != 0 => "I8087",
            _ => "Not specified"
        };
        
        md.Add("\r\n### Hardware/Software");
        md.Add($" - Operating system: `{os}`");
        md.Add($" - CPU architecture: `{cpu}`");
        md.Add($" - LINK.EXE version: {_manager.NeHeader.NE_LinkerVersion}.{_manager.NeHeader.NE_LinkerRevision}");
        if (_manager.NeHeader.NE_LinkerVersion < 5)
            md.Add("> [!WARNING]\r\n>LINK.EXE 4.0 and earlier has another logic for EntryPoints table. You have a risk of wrong bytes interpretation");
        
        if (_manager.NeHeader.NE_WindowsVersionMajor > 0)
            md.Add($" - Microsoft Windows version: {_manager.NeHeader.NE_WindowsVersionMajor}.{_manager.NeHeader.NE_WindowsVersionMinor}");
        
        md.Add("\r\n### Loader requirements");
        
        md.Add($" - Heap=`{_manager.NeHeader.NE_Heap:X4}`");
        md.Add($" - Stack=`{_manager.NeHeader.NE_Stack:X4}`");
        md.Add($" - Swap area=`{_manager.NeHeader.NE_SwapArea:X4}`");
        
        md.Add($" - DOS/2 `CS:IP={_manager.MzHeader.cs:X}:{_manager.MzHeader.ip:X4}`");
        md.Add($" - DOS/2 `SS:SP={_manager.MzHeader.ss:X}:{_manager.MzHeader.sp:X4}`");
        var cs = _manager.NeHeader.NE_CsIp >> 16;
        var ip = _manager.NeHeader.NE_CsIp & 0xFFFF;
        
        md.Add($" - Win16-OS/2 `CS:IP={cs:X4}:{ip:X4}` (hex)"); // <-- handle it
        md.Add($" - Win16-OS/2 `SS:SP={(_manager.NeHeader.NE_SsSp >> 16):X4}:{(_manager.NeHeader.NE_SsSp & 0xFFFF):X4}` (hex)"); // <-- handle it
        md.Add($"> [!INFO]\r\n> Segmented EXE Header holds on relative EntryPoint address.\r\n> EntryPoint stores in [#{cs}](decimal) segment with 0x{ip:X} offset");
        
        md.Add("\r\n### Entities summary");
        md.Add($"1. Number of Segments - `{_manager.NeHeader.NE_SegmentsCount}`");
        md.Add($"2. Number of Entry Bundles - `{_manager.NeHeader.NE_EntriesCount}`");
        md.Add($"3. Number of Moveable Entries - `{_manager.NeHeader.NE_MovableEntriesCount}`");
        md.Add($"4. Number of Automatic Data segments - `{_manager.NeHeader.NE_AutoSegment}`");
        md.Add($"5. Number of Resources - `{_manager.NeHeader.NE_ResourcesCount}`");
        md.Add($"6. Number of NonResident names - `{_manager.NeHeader.NE_NonResidentNamesCount}`");
        md.Add($"7. Number of Module References - `{_manager.NeHeader.NE_ModReferencesCount}`");
        
        // TODO: make app flags/program flags
        
        if (_manager.NeHeader.NE_FlagOthers != 0)
        {
            // TODO: make support of OS/2 loader flags
            md.Add("### OS/2 Module Characteristics");
        }
        
        Characteristics = md.ToArray();
    }
    
    private static string OnlyAscii(string target)
    {
        // exclude special chars from string
        return new string(target.Where(c => char.IsAscii(c) && c != '\0').ToArray());
    }
}