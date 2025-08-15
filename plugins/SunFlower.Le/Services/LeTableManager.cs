using System.Data;
using System.Net.NetworkInformation;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Le.Headers;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Models.Le;
using Object = SunFlower.Le.Headers.Le.Object;

namespace SunFlower.Le.Services;

public class LeTableManager
{
    private LeDumpManager _manager;

    public DataTable[] Headers { get; set; } = [];
    public List<Region> ObjectRegions { get; set; } = [];
    public DataTable ResidentNames { get; set; } = new("Resident Names Table");
    public DataTable NonResidentNames { get; set; } = new("NonResident Names Table");
    public string[] ImportedNames { get; set; } = [];
    public string[] ImportedProcedures { get; set; } = [];
    public DataTable FixupPages { get; set; } = new("Fixup Pages Table");
    public DataTable[] FixupRecords { get; set; } = [];
    public List<string> Characteristics { get; set; } = [];
    public List<Region> EntryTableRegions { get; set; } = [];
    public List<Region> NamesRegions { get; set; } = [];
    // view logic problem
    
    public LeTableManager(LeDumpManager manager)
    {
        _manager = manager;
        MakeHeaders(_manager.MzHeader, _manager.LeHeader);
        MakeNames();
        MakeObjectTables();
        MakeEntryTable();
        MakeFixupTables(); // eternal suffering
        MakeCharacteristics();
    }

    private void MakeHeaders(MzHeader mz, LeHeader le)
    {
        List<DataTable> tables =
        [
            MakeMzHeader(mz),
            MakeLeHeader(le)
        ];
        
        Headers = tables.ToArray();
    }

    private DataTable MakeMzHeader(MzHeader mz)
    {
        DataTable table = new("DOS/2 Extended Header")
        {
            Columns = { "Segment Name", "Value" }
        };
        table.Rows.Add(nameof(mz.e_sign), "0x" + mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_lastb), "0x" + mz.e_lastb.ToString("X"));
        table.Rows.Add(nameof(mz.e_fbl), "0x" + mz.e_fbl.ToString("X"));
        table.Rows.Add(nameof(mz.e_relc), "0x" + mz.e_relc.ToString("X"));
        table.Rows.Add(nameof(mz.e_pars), "0x" + mz.e_pars.ToString("X"));
        table.Rows.Add(nameof(mz.e_minep), "0x" + mz.e_minep.ToString("X"));
        table.Rows.Add(nameof(mz.e_maxep), "0x" + mz.e_maxep.ToString("X"));
        table.Rows.Add(nameof(mz.ss), "0x" + mz.ss.ToString("X"));
        table.Rows.Add(nameof(mz.sp), "0x" + mz.sp.ToString("X"));
        table.Rows.Add(nameof(mz.e_check), "0x" + mz.e_check.ToString("X"));
        table.Rows.Add(nameof(mz.ip), "0x" + mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), "0x" + mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_reltableoff), "0x" + mz.e_reltableoff.ToString("X"));
        table.Rows.Add(nameof(mz.e_overnum), "0x" + mz.e_overnum.ToString("X"));
        table.Rows.Add(nameof(mz.e_oemid), "0x" + mz.e_oemid.ToString("X"));
        table.Rows.Add(nameof(mz.e_oeminfo), "0x" + mz.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfanew), "0x" + mz.e_lfanew.ToString("X"));

        return table;
    }

    private DataTable MakeLeHeader(LeHeader header)
    {
        DataTable table = new("Linear Executable (Flat) Header")
        {
            Columns = { "Segment Name", "Value" }
        };
        
        table.Rows.Add(nameof(LeHeader.LE_ID), $"0x{header.LE_ID:X}");
        table.Rows.Add(nameof(LeHeader.LE_ByteOrder), $"0x{header.LE_ByteOrder:X}");
        table.Rows.Add(nameof(LeHeader.LE_WordOrder), $"0x{header.LE_WordOrder:X}");
        table.Rows.Add(nameof(LeHeader.LE_Format), $"0x{header.LE_Format:X}");
        table.Rows.Add(nameof(LeHeader.LE_CPU), $"0x{header.LE_CPU:X}");
        table.Rows.Add(nameof(LeHeader.LE_OS), $"0x{header.LE_OS:X}");
        table.Rows.Add(nameof(LeHeader.LE_VersionMajor), $"0x{header.LE_VersionMajor:X}");
        table.Rows.Add(nameof(LeHeader.LE_VersionMinor), $"0x{header.LE_VersionMinor:X}");
        table.Rows.Add(nameof(LeHeader.LE_Type), $"0x{header.LE_Type:X}");
        table.Rows.Add(nameof(LeHeader.LE_Pages), $"0x{header.LE_Pages:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryCS), $"0x{header.LE_EntryCS:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryEIP), $"0x{header.LE_EntryEIP:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntrySS), $"0x{header.LE_EntrySS:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryESP), $"0x{header.LE_EntryESP:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageSize), $"0x{header.LE_PageSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_LastBytes), $"0x{header.LE_LastBytes:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupSize), $"0x{header.LE_FixupSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupChk), $"0x{header.LE_FixupChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_LoaderSize), $"0x{header.LE_LoaderSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_LoaderChk), $"0x{header.LE_LoaderChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_ObjOffset), $"0x{header.LE_ObjOffset:X}");
        table.Rows.Add(nameof(LeHeader.LE_ObjNum), $"0x{header.LE_ObjNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageMap), $"0x{header.LE_PageMap:X}");
        table.Rows.Add(nameof(LeHeader.LE_IterateMap), $"0x{header.LE_IterateMap:X}");
        table.Rows.Add(nameof(LeHeader.LE_Resource), $"0x{header.LE_Resource:X}");
        table.Rows.Add(nameof(LeHeader.LE_ResourceNum), $"0x{header.LE_ResourceNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_ResidentNames), $"0x{header.LE_ResidentNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryTable), $"0x{header.LE_EntryTable:X}");
        table.Rows.Add(nameof(LeHeader.LE_Directives), $"0x{header.LE_Directives:X}");
        table.Rows.Add(nameof(LeHeader.LE_DirectivesNum), $"0x{header.LE_DirectivesNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_Fixups), $"0x{header.LE_Fixups:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupsRec), $"0x{header.LE_FixupsRec:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportModNames), $"0x{header.LE_ImportModNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportModNum), $"0x{header.LE_ImportModNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportNames), $"0x{header.LE_ImportNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageChk), $"0x{header.LE_PageChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_Data), $"0x{header.LE_Data:X}");
        table.Rows.Add(nameof(LeHeader.LE_PreLoadNum), $"0x{header.LE_PreLoadNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneRes), $"0x{header.LE_NoneRes:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneResSize), $"0x{header.LE_NoneResSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneResChk), $"0x{header.LE_NoneResChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_AutoDS), $"0x{header.LE_AutoDS:X}");
        table.Rows.Add(nameof(LeHeader.LE_Debug), $"0x{header.LE_Debug:X}");
        table.Rows.Add(nameof(LeHeader.LE_DebugSize), $"0x{header.LE_DebugSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_PreLoadInstNum), $"0x{header.LE_PreLoadInstNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_DemandInstNum), $"0x{header.LE_DemandInstNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_HeapExtra), $"0x{header.LE_HeapExtra:X}");

        return table;
    }

    private void MakeNames()
    {
        var residentHeader = "### Resident Names Table";
        var notResidentHeader = "### NonResident Names Table";

        var residentContent = "The resident name table is kept resident in system memory while the module is loaded. It is intended to contain the exported entry point names that are frequently dynamicaly linked to by name.";
        var notResidentContent = "Non-resident names are not kept in memory and are read from the EXE file when a dynamic link reference is made.";
        
        if (_manager.ResidentNames.Count == 0)
        {
            residentContent = "`<missing next information>`";
            goto __notResidentNames;
        }
        ResidentNames.Columns.Add("Name:s");
        ResidentNames.Columns.Add("Ordinal:2");
        ResidentNames.Columns.Add("Size:1");
        foreach (var name in _manager.ResidentNames)
        {
            ResidentNames.Rows.Add(name.Name, name.Ordinal, "0x" + name.Size.ToString("X2"));
        }
        NamesRegions.Add(new Region(residentHeader, residentContent, ResidentNames));
        
        __notResidentNames:
        if (_manager.NonResidentNames.Count == 0)
            goto __imports;
        
        NonResidentNames.Columns.Add("Name:s");
        NonResidentNames.Columns.Add("Ordinal:2");
        NonResidentNames.Columns.Add("Size:1");
        foreach (var name in _manager.NonResidentNames)
        {
            NonResidentNames.Rows.Add(name.Name, name.Ordinal, "0x" + name.Size.ToString("X2"));
        }
        NamesRegions.Add(new Region(notResidentHeader, notResidentContent, NonResidentNames));
        
        __imports:
        if (_manager.ImportingModules.Count == 0)
            return;
        List<string> names = [];
        
        names.AddRange(_manager.ImportingModules.Select(function => function.Name));
        ImportedNames = names.ToArray();
        
        if (_manager.ImportingProcedures.Count == 0)
            return;

        names = [];
        names.AddRange(_manager.ImportingProcedures.Select(f => f.Name));

        ImportedProcedures = names.ToArray();
    }

    private void MakeObjectTables()
    {
        DataTable objectsTable = new("Objects Table");
        const string objectHead = "### Objects Table";
        const string objectContent = "Meaning of objects in ObjectTable are the same with sections of modern executable binaries for a first time. " +
                                     "The number of entries in the Object Table is given by the # Objects in Module field in the linear " +
                                     "\nEXE header. Entries in the Object Table are numbered starting from one.";
        
        objectsTable.Columns.Add("#");
        objectsTable.Columns.Add("Name:s");
        objectsTable.Columns.Add("VirtualSize:4");
        objectsTable.Columns.Add("RelBase:4");
        objectsTable.Columns.Add("FlagsMask:4");
        objectsTable.Columns.Add("PageMapIndex:4");
        objectsTable.Columns.Add("PageMapEntries:4");
        objectsTable.Columns.Add("Unknown:4");
        objectsTable.Columns.Add("Flags:s");

        var counter = 1;
        foreach (var table in _manager.Objects)
        {
            var text = table
                .ObjectFlags
                .Aggregate("", (current, s) => current + $"`{s}` ");
            var name = Object.GetSuggestedNameByPermissions(table);
            
            objectsTable.Rows.Add(
                counter,
                name,
                "0x" + table.VirtualSegmentSize.ToString("X8"),
                "0x" + table.RelocationBaseAddress.ToString("X8"),
                "0x" + table.ObjectFlagsMask.ToString("X8"),
                "0x" + table.PageMapIndex.ToString("X8"),
                "0x" + table.PageMapEntries.ToString("X8"),
                table.Unknown.ToString("X8"),
                text
            );

            counter++;
        }
        ObjectRegions.Add(new Region(objectHead, objectContent, objectsTable));

        const string objectPageHead = "### Object Pages";
        const string objectPageContent = "The object page table specifies where in the EXE file a page can be found " +
                                         "for a given object and specifies per-page attributes. " +
                                         "The object table entries are ordered by logical page in the object table. " +
                                         "In other words the object table entries are sorted based on the object page table index value. ";
        
        DataTable objectPages = new();
        objectPages.Columns.Add("HighPage:2");
        objectPages.Columns.Add("LowPage:2");
        objectPages.Columns.Add("Flags:1");
        objectPages.Columns.Add("RealOffset:8");
        objectPages.Columns.Add("TranslatedFlags:s");

        foreach (var page in _manager.ObjectPages)
        {
            var flags = page
                .Flags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");

            objectPages.Rows.Add(
                "0x" + page.Page.HighPage.ToString("X4"),
                "0x" + page.Page.LowPage.ToString("X4"),
                "0x" + page.Page.Flags.ToString("X2"),
                "0x" + page.RealOffset.ToString("X16"), 
                flags
            );
        }
        ObjectRegions.Add(new Region(objectPageHead, objectPageContent, objectPages));
    }

    private void MakeFixupTables()
    {
        FixupPages.Columns.Add("Index");
        FixupPages.Columns.Add("Position (hex)");

        for (var i = 0; i < _manager.FixupPagesOffsets.Count; i++)
        {
            FixupPages.Rows.Add($"#{i+1}", _manager.FixupPagesOffsets[i]);
        }
        
        // uh... oh...
        DataTable rawTable = new("Raw Fixup records table")
        {
            Columns =
            {
                "ATP (hex)", 
                "RTP (hex)", 
                "Internal& (hex)", 
                "AddVal (hex)", 
                "ExtraData (hex)", 
                "Mod#", 
                "Name offset (hex)", 
                "Ordinal"
            }
        };

        foreach (var record in _manager.FixupRecords)
        {
            rawTable.Rows.Add(
                record.Record.AddressType.ToString("X"),
                record.Record.RelocationType.ToString("X"),
                record.Record.TargetObject.ToString("X"),
                record.Record.AddValue.ToString("X"),
                record.Record.ExtraData.ToString("X"),
                record.Record.ModuleIndex.ToString("X"),
                record.Record.NameOffset.ToString("X"),
                record.Record.Ordinal);
        }
        
        DataTable table = new("Processed Fixup records table")
        {
            Columns =
            {
                "Page#",
                "Target",
                "Add. Value",
                "ExtraData",
                "OSFixup",
                "Ordinal",
                "Name",
                "ATP",
                "RTP"
            }
        };
        
        foreach (var model in _manager.FixupRecords)
        {
            var atp = model.AddressTypeFlags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");
            var rtp = model.RecordTypeFlags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");

            table.Rows.Add(
                model.PageIndex.ToString(),
                model.Record.TargetObject.ToString("X"),
                model.Record.AddValue.ToString("X"),
                model.Record.ExtraData.ToString("X"),
                model.Record.OsFixup.ToString("X"),
                model.ImportingOrdinal,
                $"`{OnlyAscii(model.ImportingName)}`",
                atp,
                rtp
                );
        }

        FixupRecords = [rawTable, table];
    }

    private void MakeEntryTable()
    {
        var bundleCounter = 1;
        foreach (var bundle in _manager.EntryBundles)
        {
            var head = $"### EntryTable Bundle #{bundleCounter}";
            StringBuilder contentBuilder = new();
            contentBuilder.AppendLine(
                "The entry table contains object and offset information that is used " +
                "to resolve fixup references to the entry points within this module. " +
                "Not all entry points in the entry table will be exported, some entry points " +
                "will only be used within the module. An ordinal number is used to index into the entry table. " +
                "The entry table entries are numbered starting from one.");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine($"Bundle Index: `{bundle.EntryBundleIndex}`\r\n");
            contentBuilder.AppendLine($"Object Index: `{bundle.ObjectIndex}`");

            contentBuilder.AppendLine("Bundle Flags: ");
            contentBuilder.AppendLine($" - Usage=`{bundle.EntriesUsage}`");
            contentBuilder.AppendLine($" - Per-Entry Capacity=`{bundle.EntriesSafeSize}`");
            
            contentBuilder.AppendLine("\r\n");
            contentBuilder.AppendLine($"Rows affected: {bundle.EntriesCount}");
            DataTable entries = new()
            {
                Columns = { "Ordinal:4", "Entry:s", "Object:s", "Offset:4", "Flag:2" }
            };
            
            foreach (var entry in bundle.Entries)
            {
                entries.Rows.Add(
                    "@" + entry.Ordinal,
                    entry.EntryType,
                    entry.ObjectType,
                    "0x" + entry.Offset.ToString("X4"),
                    "0x" + entry.Flag.ToString("X2")
                    );
            }
            EntryTableRegions.Add(new Region(head, contentBuilder.ToString(), entries));
            bundleCounter++;
        }
    }
    private void MakeCharacteristics()
    {
        List<string> md = [];
        var description = _manager.NonResidentNames.Count > 0 ? _manager.NonResidentNames[0].Name : "`<missing>`";
        var name = _manager.ResidentNames.Count > 0 ? _manager.ResidentNames[0].Name : "`<name_missing>`";
        md.Add("### Program Header information");
        md.Add($"Project Name: {_manager.ResidentNames[0].Name}");
        md.Add($"Description: \"{description}\"");
        md.Add("Target CPU: " + GetCpuType(_manager.LeHeader.LE_CPU));
        md.Add("Target OS: " + GetOsType(_manager.LeHeader.LE_OS));
        md.Add($"Module Version: {_manager.LeHeader.LE_VersionMajor}.{_manager.LeHeader.LE_VersionMinor}");
        
        md.Add($"Resolved \"{_manager.ResidentNames[0].Name}\" module flags:");
        foreach (var flag in GetModuleFlags(_manager.LeHeader.LE_Type))
        {
            md.Add($" - `{flag}`");
        }
        
        if (_manager.LeHeader.LE_ID is 0x584c or 0x4c58)
            md.Add("> [!WARNING]\r\n> Signature of FLAT EXEC header is `LX`. This FLAT EXEC binary contains **only 32-bit code** and has unknown structures for this plugin. You have a risk of corrupted bytes-interpretation.");
        
        md.Add($"\r\n### `{name}` Loader requirements");
        md.Add("This summary contains hexadecimal values from FLAT EXEC header.");
        md.Add($" - HeapExtra=`{_manager.LeHeader.LE_HeapExtra:X}`");
        
        md.Add($" - DOS/2 `CS:IP=0x{_manager.MzHeader.cs:X4}:0x{_manager.MzHeader.ip:X4}`");
        md.Add($" - DOS/2 `SS:SP=0x{_manager.MzHeader.ss:X4}:0x{_manager.MzHeader.sp:X4}`");
        
        var cs = _manager.LeHeader.LE_EntryCS;
        var ip = _manager.LeHeader.LE_EntryEIP;
        md.Add($" - Win32s-OS/2 `CS:EIP=0x{cs:X8}:0x{ip:X8}`"); // <-- handle it
        md.Add($" - Win32s-OS/2 `SS:ESP=0x{_manager.LeHeader.LE_EntrySS:X8}:0x{_manager.LeHeader.LE_EntryESP:X8}`");
        
        md.Add($"> [!INFO]\r\n> Flat EXE Header holds on relative EntryPoint address.\r\n> EntryPoint stores in [#{cs}](decimal) object with `EIP=0x{ip:X}` offset");
        
        md.Add($"\r\n### `{name}` Entities summary");
        md.Add("This summary contains decimal values took from FLAT EXEC Header model.");
        md.Add($"1. Number of Objects - `{_manager.LeHeader.LE_ObjNum}`");
        md.Add($"2. Number of Importing Modules - `{_manager.LeHeader.LE_ImportModNum}`");
        md.Add($"3. Number of Preload Pages - `{_manager.LeHeader.LE_PreLoadNum}`");
        md.Add($"4. Number of Automatic Data segments - `{_manager.LeHeader.LE_AutoDS}`");
        md.Add($"5. Number of Resources - `{_manager.LeHeader.LE_ResourceNum}`");
        md.Add($"6. Number of NonResident names - `{_manager.LeHeader.LE_NoneResSize}`");
        md.Add($"7. Number of Directives - `{_manager.LeHeader.LE_DirectivesNum}`");
        md.Add($"8. Number of Demand Instances - `{_manager.LeHeader.LE_DemandInstNum}`");
        
        if (_manager.DriverHeader.LE_DDKMajor > 0)
            MakeDriverCharacteristics(ref md);
        
        Characteristics = md;
    }

    private void MakeDriverCharacteristics(ref List<string> md)
    {
        md.Add("\r\n### Windows `VxD` Driver Header");
        md.Add($"Requires Microsoft Windows {_manager.DriverHeader.LE_DDKMajor}.{_manager.DriverHeader.LE_DDKMinor} and earlier versions.");
        md.Add($"Driver resources stores at: 0x{_manager.DriverHeader.LE_WindowsResOffset:X}");
        
        // TODO: make VxdResources struct / make VxdDdb struct.
        
    }
    private static string GetCpuType(ushort cpuType)
    {
        return cpuType switch
        {
            LeHeader.LeCpu286 => "Intel 286",
            LeHeader.LeCpu386 => "Intel 386",
            LeHeader.LeCpu486 => "Intel 486",
            LeHeader.LeCpu586 => "Intel Pentium",
            LeHeader.LeCpuI860 => "Intel i860",
            LeHeader.LeCpuN11 => "N11",
            LeHeader.LeCpuR2000 => "MIPS R2000",
            LeHeader.LeCpuR6000 => "MIPS R6000",
            LeHeader.LeCpuR4000 => "MIPS R4000",
            _ => $"Unknown: (0x{cpuType:X4})"
        };
    }
    
    private static string GetOsType(ushort osType)
    {
        return osType switch
        {
            LeHeader.LeOsOs2 => "OS/2",
            LeHeader.LeOsWindows => "Windows",
            LeHeader.LeOsDos4 => "DOS 4.x",
            LeHeader.LeOsWin386 => "Windows 386",
            _ => $"Unknown OS (0x{osType:X4})"
        };
    }
    
    private static string[] GetModuleFlags(uint flags)
    {
        var result = new List<string>();

        if ((flags & LeHeader.LeTypeInitPer) != 0)
            result.Add("Initialise per-process library");

        if ((flags & LeHeader.LeTypeIntFixup) != 0)
            result.Add("No internal fixups");

        if ((flags & LeHeader.LeTypeExtFixup) != 0)
            result.Add("No external fixups");

        if ((flags & LeHeader.LeTypeNoLoad) != 0)
            result.Add("Module not loadable");

        if ((flags & LeHeader.LeTypeDll) != 0)
            result.Add("DLL module");

        if (result.Count == 0)
            result.Add("No special flags");

        return result.ToArray();
    }
    
    private static string OnlyAscii(string str)
    {
        var processed = str.Where(char.IsAscii).ToList();
        
        return new string(processed.ToArray());
    }
}