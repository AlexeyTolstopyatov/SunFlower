using System.Data;
using System.Numerics;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
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
        MakeImportRegions();
    }

    public List<Region> NamesRegions { get; } = [];
    public List<Region> EntryBundlesRegions { get; set; } = [];
    public List<Region> SegmentRegions { get; } = [];
    public List<DataTable> Headers { get; set; } = [];
    public string[] Characteristics { get; set; } = [];
    public List<Region> ImportRegions { get; set; } = [];
    public List<Region> ModulesRegion { get; } = [];

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
        table.Rows.Add(nameof(mz.e_crc), mz.e_crc.ToString("X"));
        table.Rows.Add(nameof(mz.ip), mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfarlc), mz.e_lfarlc.ToString("X"));
        table.Rows.Add(nameof(mz.e_ovno), mz.e_ovno.ToString("X"));
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

    private void MakeImportRegions()
    {
        // iterate each dictionary record
        foreach (var importPair in _manager.ImportModels)
        {
            string head = $"### Resolved Imports of {FlowerReport.SafeString(importPair.Key)}";
            string content = "16-bit Imports processor bases at per-segment relocations. " +
                             "If segment has special bit in the byte-mask, next services will iterate preprocessed relocations records";
            DataTable table = new()
            {
                Columns = { "Name", "Ordinal" }
            };
            
            foreach (var import in importPair.Value)
            {
                table.Rows.Add(FlowerReport.SafeString(import.Procedure), import.Ordinal);
            }

            ImportRegions.Add(new(head, content, table));
        }
    }
    private void MakeSegmentsTable()
    {
        if (_manager.Segments.Count == 0)
            return;
        
        DataTable segs = new("Segments Table")
        {
            Columns =
            {
                FlowerReport.ForColumn("Type", typeof(string)),
                FlowerReport.ForColumn("#Segment", typeof(uint)),
                FlowerReport.ForColumn("Offset", typeof(ushort)),
                FlowerReport.ForColumn("Length", typeof(ushort)),
                FlowerReport.ForColumn("Flags", typeof(ushort)),
                FlowerReport.ForColumn("Minimum allocation", typeof(ushort)),
                FlowerReport.ForColumn("Flags", typeof(string))
            }
        };
        
        foreach (var segmentDump in _manager.Segments)
        {
            var array = segmentDump
                .Characteristics
                .Aggregate("", (current, characteristic) => current + characteristic + " ");
            
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

        var head = "### Segments Table";
        var content = 
            "The segment table contains an entry for each segment in the executable file.\n " +
            "The number of segment table entries are defined in the segmented EXE header\n. " +
            "The first entry in the segment table is segment number 1. " +
            "The following is the structure of a segment table entry. ";
        
        SegmentRegions.Add(new Region(head, content, segs));
    }

    private void MakeEntryPointsTable()
    {
        if (_manager.EntryBundles.Count == 0)
            return;
        
        var counter = 1;
        var content = 
            "The linker forms bundles in the most dense manner it can, \n" +
            "under the restriction that it cannot reorder entry points to improve bundling. " +
            "\nThe reason for this restriction is that other .EXE files may refer to entry points within this bundle by their ordinal number.";
        
        var tables = new List<Region>();
        
        foreach (NeEntryBundle bundle in _manager.EntryBundles)
        {
            var head = $"### EntryTable Bundle #{counter}";
            
            DataTable entries = new($"EntryTable Bundle #{counter}")
            {
                Columns =
                {
                    FlowerReport.ForColumn("Ordinal", typeof(ushort)),
                    FlowerReport.ForColumn("Offset", typeof(ushort)),
                    FlowerReport.ForColumn("Segment", typeof(ushort)),
                    FlowerReport.ForColumn("Entry", typeof(string)),
                    FlowerReport.ForColumn("Data", typeof(string)),
                    FlowerReport.ForColumn("Type", typeof(string))
                }
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
            tables.Add(new Region(head, content, entries));
        }

        EntryBundlesRegions = tables;
    }

    private void MakeModuleReferencesTable()
    {
        if (_manager.ModuleReferences.Count == 0)
            return;

        var head = "### Module References";
        var content = 
            "The module-reference table follows the resident-name table. " +
            "Each entry contains an offset for the module-name string within the imported names table; " +
            "each entry is 2 bytes long.";
        
        DataTable modres = new("Module References")
        {
            Columns =
            {
                FlowerReport.ForColumn("Reference#", typeof(int)), 
                FlowerReport.ForColumn("Offset", typeof(ushort))
            }
        };
        for (var i = 0; i < _manager.ModuleReferences.Count; ++i)
        {
            modres.Rows.Add(
                i + 1,
                $"0x{_manager.ModuleReferences[i]:X4}"
            );
        }

        ModulesRegion.Add(new Region(head, content, modres));
    }

    private void MakeSegmentRelocations()
    {
        var sexWithRelocations = _manager.Segments.Where(s => s.Relocations.Count > 0).Distinct().ToList();
        var content = 
            "The location and size of the per-segment data is defined in the segment table entry for the segment. " +
            "If the segment has relocation fixups, as defined in the segment table entry flags, they directly " +
            "follow the segment data in the file.";
        foreach (var segment in sexWithRelocations)
        {
            var head = $"### Relocations Table for Segment #{segment.SegmentNumber} ({segment.Type})";
            DataTable table = new()
            {
                Columns =
                {
                    FlowerReport.ForColumn("ATP", typeof(byte)),
                    FlowerReport.ForColumn("RTP", typeof(byte)),
                    FlowerReport.ForColumn("RTP", typeof(string)),
                    FlowerReport.ForColumn("Additive?", typeof(bool)),
                    FlowerReport.ForColumn("OffsetInSeg", typeof(ushort)),
                    FlowerReport.ForColumn("SegType", typeof(ushort)),
                    FlowerReport.ForColumn("Target", typeof(ushort)),
                    FlowerReport.ForColumn("TargetType", typeof(string)),
                    FlowerReport.ForColumn("Mod#", typeof(string)),
                    FlowerReport.ForColumn("Name", typeof(ushort)),
                    FlowerReport.ForColumn("Ordinal", typeof(ushort)),
                    FlowerReport.ForColumn("Fixup", typeof(string))
                }
            };

            foreach (var rel in segment.Relocations)
            {
                // prepare table
                table.Rows.Add(
                    "0x" + rel.AddressType.ToString("X"), 
                    "0x" + rel.RelocationFlags.ToString("X"), 
                    $"[{rel.RelocationType}]", 
                    $"[{rel.IsAdditive}]",
                    "0x" + rel.OffsetInSegment.ToString("X"), 
                    $"{rel.SegmentType}", 
                    "0x" + rel.Target.ToString("X"), 
                    $"[{rel.TargetType}]", 
                    "0x" + rel.ModuleIndex.ToString("X"),
                    "@" + rel.Ordinal, 
                    "0x" + rel.NameOffset.ToString("X"),
                    rel.Fixup);
            }
            // per-segment relocation regions
            SegmentRegions.Add(new Region(head, content, table));
        }
    }
    private void MakeNames()
    {
        if (_manager.NonResidentNames.Count == 0)
            return;
        var content = 
            "The resident-name table follows the resource table, " +
            "and contains this module's name string and resident exported procedure name strings. " +
            "The first string in this table is this module's name. \n\n" +
            "The nonresident-name table follows the entry table, and contains a " +
            "module description and nonresident exported procedure name strings. " +
            "The first string in this table is a module description. These name " +
            "strings are case-sensitive and are not null-terminated.";
        
        DataTable nonres = new()
        {
            Columns = { "Count", "Name", "Ordinal", "Name Table" }
        };
        foreach (var neExportDump in _manager.NonResidentNames)
        {
            nonres.Rows.Add(
                neExportDump.Count,
                FlowerReport.SafeString(neExportDump.String),
                "@" + neExportDump.Ordinal,
                "[Not resident]"
            );
        }

        foreach (var export in _manager.ResidentNames)
        {
            nonres.Rows.Add(
                export.Count,
                FlowerReport.SafeString(export.String),
                "@" + export.Ordinal,
                "[Resident]"
            );
        }

        NamesRegions.Add(new Region("### Resident And NonResident Names", content, nonres));
    }
    private void MakeCharacteristics()
    {
        List<string> md = [];
        
        //md.Add("_Main information details took from Windows New segmented EXE header (called `IMAGE_OS2_HEADER` in Win32 API)_\r\n");
        md.Add("\r\n# Image");
        md.Add($"Project Name: {FlowerReport.SafeString(_manager.ResidentNames[0].String)}"); // <-- first name always project-name
        md.Add($"Description: {FlowerReport.SafeString(_manager.NonResidentNames[0].String)}");
        
        var os = _manager.NeHeader.NE_OS switch
        {
            0x0 => "Any OS supported", // set for *.FON. means "any OS supported"  
            0x1 => "OS/2",
            0x2 => "Windows/286",
            0x3 => "DOS/4",
            0x4 => "Windows/386",
            0x5 => "BoSS",
            _ => $"Unknown 0x{_manager.NeHeader.NE_OS:X}" // <-- really don't know how handle it
        };

        var cpu = _manager.NeHeader.NE_Flags switch
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
        
        md.Add("\r\n## Loader requirements");
        
        md.Add($" - Heap=`{_manager.NeHeader.NE_Heap:X4}`");
        md.Add($" - Stack=`{_manager.NeHeader.NE_Stack:X4}`");
        md.Add($" - Swap area=`{_manager.NeHeader.NE_SwapArea:X4}`");
        
        md.Add($" - DOS/2 `CS:IP`={FlowerReport.FarHexString(_manager.MzHeader.cs, _manager.MzHeader.ip, true)}");
        md.Add($" - DOS/2 `SS:SP`={FlowerReport.FarHexString(_manager.MzHeader.ss, _manager.MzHeader.sp, true)}");
        
        var cs = _manager.NeHeader.NE_CsIp >> 16;
        var ip = _manager.NeHeader.NE_CsIp & 0xFFFF;
        var ss = _manager.NeHeader.NE_SsSp >> 16;
        var sp = _manager.NeHeader.NE_SsSp & 0xFFFF;
        
        md.Add($" - Win16-OS/2 `CS:IP`={FlowerReport.FarHexString((ushort)cs, (ushort)ip, true)}"); // <-- handle it
        md.Add($" - Win16-OS/2 `SS:SP`={FlowerReport.FarHexString((ushort)ss, (ushort)sp, true)}"); // <-- handle it
        md.Add($"> [!TIP]\r\n> Segmented EXE Header holds on relative EntryPoint address.\r\n> EntryPoint stores in [#{cs}](decimal) segment with 0x{ip:X} offset");
        
        md.Add("\r\n## Entities summary");
        md.Add($"1. Number of Segments - `{_manager.NeHeader.NE_SegmentsCount}`");
        md.Add($"2. Number of Entry Bundles - `{_manager.NeHeader.NE_EntriesCount}`");
        md.Add($"3. Number of Moveable Entries - `{_manager.NeHeader.NE_MovableEntriesCount}`");
        md.Add($"4. Number of Automatic Data segments - `{_manager.NeHeader.NE_AutoSegment}`");
        md.Add($"5. Number of Resources - `{_manager.NeHeader.NE_ResourcesCount}`");
        md.Add($"6. Number of `BYTE`s in NonResident names table - `{_manager.NeHeader.NE_NonResidentNamesCount}`");
        md.Add($"7. Number of Module References - `{_manager.NeHeader.NE_ModReferencesCount}`");
        
        // program flags 
        var p = _manager.NeHeader.NE_Flags;
        md.Add($"## Program Flags");
        md.Add("### How data is handled?");
        md.Add(@"
In 16-bit DOS/Windows terminology, `DGROUP` is a segment class that referring
to segments that are used for data.

Win16 used segmentation to permit a DLL or program to have multiple
instances along with an instance handle and manage multiple data
segments. In example: allowed one `NOTEPAD.EXE` code segment to execute
multiple instances of the notepad application.");
        if ((p & 0x0000) != 0) md.Add(" - `NO_AUTODATA`");
        if ((p & 0x0002) != 0) md.Add(" - `SINGLE_DATA` (shared among instances of the same program)");
        if ((p & 0x2000) != 0) md.Add(" - `MULTIPLE_DATA` (separate for each instance of the same program)");
        
        md.Add("### How application runs?");
        if ((p & 0x0008) != 0) md.Add(" - `PROTECTED_MODE_ONLY`");
        if ((p & 0x0004) != 0) md.Add(" - `GLOBINIT` - (global initialization)");
        
        md.Add("### Extra details?");
        if ((p & 0x2000) != 0) md.Add(" - `LINK_ERR` - (module has errors after linkage. Don't try to run it)");
        if ((p & 0x8000) != 0) md.Add(" - `LIB_MODULE` (dynamically linked module)");
        
        md.Add("## Application Flags");
        md.Add("This block (field) tells how windowing or not windowing wants to run");
        
        var a = _manager.NeHeader.NE_Flags;
        
        if ((a & 0x0080) != 0) md.Add(" - `OS2_FAMILY` (OS/2 family application. You can see OS/2 flags section)");
        if ((a & 0x0020) != 0) md.Add(" - `IMAGE_ERR` (OS doesn't want that you run it).");
        if ((a & 0x0040) != 0) md.Add(" - `NON_CONFORM` (nonconforming program)");
        
        if (_manager.NeHeader.NE_FlagOthers != 0)
        {
            md.Add("## OS/2 Flags");
            md.Add("Sunflower plugin shows this section if `e_flagothers` not zero. But I also suppose if appflags has `OS2_FAMILY`" +
                   " or `e_os` equals 0x1, what means OS/2 - you can read this section.");
            var o = _manager.NeHeader.NE_FlagOthers;
            if ((o & 0x0001) != 0) md.Add(" - `LONG_NAMES` (avoid FAT rule 8.3 convertion)");
            if ((o & 0x0002) != 0) md.Add(" - `PROTECTED_MODE` (OS/2 2.0+ protected mode application)");
            if ((o & 0x0004) != 0) md.Add(" - `PROP_FONTS` (proportional fonts)");
            if ((o & 0x0008) != 0) md.Add(" - `GANGLOAD_AREA`");
        }
        
        Characteristics = md.ToArray();
    }
}