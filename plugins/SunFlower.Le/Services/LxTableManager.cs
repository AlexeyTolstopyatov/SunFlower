using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Headers.Lx;

namespace SunFlower.Le.Services;

public class LxTableManager
{
    private LxDumpManager _manager;
    
    public List<Region> ObjectRegions { get; set; } = [];
    public List<Region> EntryTableRegions { get; set; } = [];
    public List<Region> NamesRegions { get; set; } = [];
    public List<string> Characteristics { get; set; } = [];
    public string[] ImportedNames { get; set; } = [];
    public string[] ImportedProcedures { get; set; } = [];
    
    public LxTableManager(LxDumpManager manager)
    {
        _manager = manager;
        // list of init queue
        MakeCharacteristics();
        MakeObjectTables();
        MakeNames();
        MakeEntryTable();
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
            var name = Headers.Lx.Object.GetSuggestedNameByPermissions(table);
            
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

        foreach (var page in _manager.Pages)
        {
            var flags = page
                .Flags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");

            objectPages.Rows.Add(
                "0x" + page.Page.HighPage.ToString("X4"),
                "0x" + page.Page.LowPage.ToString("X4"),
                "0x" + page.Page.Flags.ToString("X2"),
                flags
            );
        }
        ObjectRegions.Add(new Region(objectPageHead, objectPageContent, objectPages));
    }
    private void MakeNames()
    {
        DataTable residentNames = new();
        
        var residentHeader = "### Resident Names Table";
        var notResidentHeader = "### NonResident Names Table";

        var residentContent = "The resident name table is kept resident in system memory while the module is loaded. It is intended to contain the exported entry point names that are frequently dynamicaly linked to by name.";
        var notResidentContent = "Non-resident names are not kept in memory and are read from the EXE file when a dynamic link reference is made.";
        
        if (_manager.ResidentNames.Count == 0)
        {
            residentContent = "`<missing next information>`";
            goto __notResidentNames;
        }
        residentNames.Columns.Add("Name:s");
        residentNames.Columns.Add("Ordinal:2");
        residentNames.Columns.Add("Size:1");
        foreach (var name in _manager.ResidentNames)
        {
            residentNames.Rows.Add(name.String, name.Ordinal, "0x" + name.Size.ToString("X2"));
        }
        NamesRegions.Add(new Region(residentHeader, residentContent, residentNames));
        
        __notResidentNames:
        if (_manager.NonResidentNames.Count == 0)
            goto __imports;
        DataTable nonResidentNames = new();
        
        nonResidentNames.Columns.Add("Name:s");
        nonResidentNames.Columns.Add("Ordinal:2");
        nonResidentNames.Columns.Add("Size:1");
        foreach (var name in _manager.NonResidentNames)
        {
            nonResidentNames.Rows.Add(name.String, name.Ordinal, "0x" + name.Size.ToString("X2"));
        }
        NamesRegions.Add(new Region(notResidentHeader, notResidentContent, nonResidentNames));
        
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
    private void MakeEntryTable()
    {
        var bundleCounter = 1;
        foreach (var bundle in _manager.EntryBundles)
        {
            var head = $"### EntryTable Bundle #{bundleCounter}";
            StringBuilder contentBuilder = new();
            
            contentBuilder.AppendLine(bundle.TypeDescription);
            contentBuilder.AppendLine($"Bundle Type: {bundle.TypeString}");
            contentBuilder.AppendLine($" - Entries=`{bundle.Count}`");
            
            contentBuilder.AppendLine("\r\n");
            contentBuilder.AppendLine($"Rows affected: {bundle.Count}");

            DataTable entries;
            switch (bundle.Type)
            {
                case EntryBundleType._16Bit:
                    entries = new()
                    {
                        Columns = { "Object#:2", "Offset:2", "Entry:s", "Flags:1", "ObjectOffsets:s" }
                    };
                    
                    foreach (var unpacked in bundle.Entries.Cast<Entry16Bit>())
                    {
                        entries.Rows.Add(
                            unpacked.ObjectNumber,
                            "0x", unpacked.Offset.ToString("X4"),
                            unpacked.EntryType,
                            "0x" + unpacked.Flags.ToString("X2"),
                            unpacked.ObjectOffsets
                        );
                    }
                    EntryTableRegions.Add(new Region(head, contentBuilder.ToString(), entries));
                    break;
                case EntryBundleType._32Bit:
                    entries = new()
                    {
                        Columns = { "Object#:2", "Offset:2", "Entry:s", "Flags:1", "ObjectOffsets:s" }
                    };
                    
                    foreach (var unpacked in bundle.Entries.Cast<Entry32Bit>())
                    {
                        entries.Rows.Add(
                            unpacked.ObjectNumber,
                            "0x", unpacked.Offset.ToString("X8"),
                            unpacked.EntryType,
                            "0x" + unpacked.Flags.ToString("X2"),
                            unpacked.ObjectOffsets
                        );
                    }
                    break;
                case EntryBundleType._286CallGate:
                    entries = new()
                    {
                        Columns = { "Object#:2", "Offset:2", "Entry:s", "Flags:1", "ObjectOffsets:s", "CallGate:2" }
                    };
                    
                    foreach (var unpacked in bundle.Entries.Cast<Entry286CallGate>())
                    {
                        entries.Rows.Add(
                            unpacked.ObjectNumber,
                            "0x", unpacked.Offset.ToString("X4"),
                            unpacked.EntryType,
                            "0x" + unpacked.Flags.ToString("X2"),
                            unpacked.ObjectOffsets,
                            "0x" + unpacked.CallGateSelector.ToString("X4")
                        );
                    }
                    break;
                case EntryBundleType.Forwarder:
                    entries = new()
                    {
                        Columns = { "@Module:2", "@Offset:4", "ObjectOffsets:s" }
                    };
                    foreach (var unpacked in bundle.Entries.Cast<EntryForwarder>())
                    {
                        entries.Rows.Add(
                            "0x" + unpacked.ModuleOrdinal.ToString("X4"),
                            "0x" + unpacked.OffsetOrOrdinal.ToString("X4"),
                            unpacked.ObjectOffsets
                        );
                    }
                    break;
                default:
                    entries = new();
                    break;
            }
            
            EntryTableRegions.Add(new Region(head, contentBuilder.ToString(), entries));
            bundleCounter++;
        }
    }
    private void MakeCharacteristics()
    {
        List<string> md = [];
        var description = _manager.NonResidentNames.Count > 0 ? _manager.NonResidentNames[0].String : "`<missing>`";
        var name = _manager.ResidentNames.Count > 0 ? _manager.ResidentNames[0].String : "`<name_missing>`";
        md.Add("### Program Header information");
        md.Add($"Project Name: {_manager.ResidentNames[0].String}");
        md.Add($"Description: \"{description}\"");
        md.Add("Target CPU: " + GetCpuType(_manager.LxHeader.e32_cpu));
        md.Add("Target OS: " + GetOsType(_manager.LxHeader.e32_os));
        md.Add($"Module Version: {_manager.LxHeader.e32_ver >> 16}.{_manager.LxHeader.e32_ver & 0xFFFF}");
        
        md.Add($"Resolved \"{_manager.ResidentNames[0].String}\" module flags:");
        foreach (var flag in GetModuleFlags(_manager.LxHeader.e32_mflags))
        {
            md.Add($" - `{flag}`");
        }
        
        if (_manager.LxHeader.e32_magic is 0x454c or 0x4c45)
            md.Add("> ![WARNING]\r\n> Signature of FLAT EXEC header is `LX`. This FLAT EXEC binary contains **only 32-bit code** and has unknown structures for this plugin. You have a risk of corrupted bytes-interpretation.");
        
        md.Add($"\r\n### `{name}` Loader requirements");
        md.Add("This summary contains hexadecimal values from FLAT EXEC header.");
        md.Add($" - Heap=`{_manager.LxHeader.e32_heapsize:X}`");
        md.Add($" - Stack={_manager.LxHeader.e32_stacksize:X}");
        md.Add($" - DOS/2 `CS:IP=0x{_manager.MzHeader.cs:X4}:0x{_manager.MzHeader.ip:X4}`");
        md.Add($" - DOS/2 `SS:SP=0x{_manager.MzHeader.ss:X4}:0x{_manager.MzHeader.sp:X4}`");
        
        var cs = _manager.LxHeader.e32_startobj;
        var ip = _manager.LxHeader.e32_eip;
        md.Add($" - Win32s-OS/2 `CS:EIP=0x{cs:X8}:0x{ip:X8}`"); // <-- handle it
        md.Add($" - Win32s-OS/2 `SS:ESP=0x{_manager.LxHeader.e32_stackobj:X8}:0x{_manager.LxHeader.e32_esp:X8}`");
        
        md.Add($"> ![TIP]\r\n> Flat EXE Header holds on relative EntryPoint address. EntryPoint stores in [#{cs}](decimal) object with `EIP=0x{ip:X}` offset");
        
        md.Add($"\r\n### `{name}` Entities summary");
        md.Add("This summary contains decimal values took from FLAT EXEC Header model.");
        md.Add($"1. Number of Objects - `{_manager.LxHeader.e32_objcnt}`");
        md.Add($"2. Number of Importing Modules - `{_manager.LxHeader.e32_impmodcnt}`");
        md.Add($"3. Number of Preload Pages - `{_manager.LxHeader.e32_preload}`");
        md.Add($"4. Number of Automatic Data segments - `{_manager.LxHeader.e32_autodata}`");
        md.Add($"5. Number of Resources - `{_manager.LxHeader.e32_cbnrestab}`");
        md.Add($"6. Number of NonResident names - `{_manager.LxHeader.e32_cbnrestab}`");
        md.Add($"7. Number of Directives - `{_manager.LxHeader.e32_dircnt}`");
        md.Add($"8. Number of Demand Instances - `{_manager.LxHeader.e32_instdemand}`");
        
        Characteristics = md;
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