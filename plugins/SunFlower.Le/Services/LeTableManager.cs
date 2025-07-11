using System.Data;
using System.Net.NetworkInformation;
using SunFlower.Le.Headers;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Models.Le;

namespace SunFlower.Le.Services;

public class LeTableManager
{
    private LeDumpManager _manager;

    public DataTable[] Headers { get; set; } = [];
    public DataTable ObjectsTable { get; set; } = new("Objects Table");
    public DataTable ObjectPages { get; set; } = new("Object Pages Table");
    public DataTable ResidentNames { get; set; } = new("Resident Names Table");
    public DataTable NonResidentNames { get; set; } = new("NonResident Names Table");
    public DataTable[] EntryTables { get; set; } = [];
    public string[] ImportedNames { get; set; } = [];
    public string[] ImportedProcedures { get; set; } = [];
    public DataTable FixupPages { get; set; } = new("Fixup Pages Table");
    public DataTable[] FixupRecords { get; set; } = [];
    public List<string> Characteristics { get; set; } = [];

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

    private DataTable MakeLeHeader(LeHeader header)
    {
        DataTable table = new("Linear Executable (Flat) Header")
        {
            Columns = { "Segment Name", "Value" }
        };
        
        table.Rows.Add(nameof(LeHeader.LE_ID), $"{header.LE_ID:X}");
        table.Rows.Add(nameof(LeHeader.LE_ByteOrder), $"{header.LE_ByteOrder:X}");
        table.Rows.Add(nameof(LeHeader.LE_WordOrder), $"{header.LE_WordOrder:X}");
        table.Rows.Add(nameof(LeHeader.LE_Format), $"{header.LE_Format:X}");
        table.Rows.Add(nameof(LeHeader.LE_CPU), $"{header.LE_CPU:X}");
        table.Rows.Add(nameof(LeHeader.LE_OS), $"{header.LE_OS:X}");
        table.Rows.Add(nameof(LeHeader.LE_Version), $"{header.LE_Version:X}");
        table.Rows.Add(nameof(LeHeader.LE_Type), $"{header.LE_Type:X}");
        table.Rows.Add(nameof(LeHeader.LE_Pages), $"{header.LE_Pages:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryCS), $"{header.LE_EntryCS:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryEIP), $"{header.LE_EntryEIP:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntrySS), $"{header.LE_EntrySS:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryESP), $"{header.LE_EntryESP:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageSize), $"{header.LE_PageSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_LastBytes), $"{header.LE_LastBytes:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupSize), $"{header.LE_FixupSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupChk), $"{header.LE_FixupChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_LoaderSize), $"{header.LE_LoaderSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_LoaderChk), $"{header.LE_LoaderChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_ObjOffset), $"{header.LE_ObjOffset:X}");
        table.Rows.Add(nameof(LeHeader.LE_ObjNum), $"{header.LE_ObjNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageMap), $"{header.LE_PageMap:X}");
        table.Rows.Add(nameof(LeHeader.LE_IterateMap), $"{header.LE_IterateMap:X}");
        table.Rows.Add(nameof(LeHeader.LE_Resource), $"{header.LE_Resource:X}");
        table.Rows.Add(nameof(LeHeader.LE_ResourceNum), $"{header.LE_ResourceNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_ResidentNames), $"{header.LE_ResidentNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_EntryTable), $"{header.LE_EntryTable:X}");
        table.Rows.Add(nameof(LeHeader.LE_Directives), $"{header.LE_Directives:X}");
        table.Rows.Add(nameof(LeHeader.LE_DirectivesNum), $"{header.LE_DirectivesNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_Fixups), $"{header.LE_Fixups:X}");
        table.Rows.Add(nameof(LeHeader.LE_FixupsRec), $"{header.LE_FixupsRec:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportModNames), $"{header.LE_ImportModNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportModNum), $"{header.LE_ImportModNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_ImportNames), $"{header.LE_ImportNames:X}");
        table.Rows.Add(nameof(LeHeader.LE_PageChk), $"{header.LE_PageChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_Data), $"{header.LE_Data:X}");
        table.Rows.Add(nameof(LeHeader.LE_PreLoadNum), $"{header.LE_PreLoadNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneRes), $"{header.LE_NoneRes:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneResSize), $"{header.LE_NoneResSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_NoneResChk), $"{header.LE_NoneResChk:X}");
        table.Rows.Add(nameof(LeHeader.LE_AutoDS), $"{header.LE_AutoDS:X}");
        table.Rows.Add(nameof(LeHeader.LE_Debug), $"{header.LE_Debug:X}");
        table.Rows.Add(nameof(LeHeader.LE_DebugSize), $"{header.LE_DebugSize:X}");
        table.Rows.Add(nameof(LeHeader.LE_PreLoadInstNum), $"{header.LE_PreLoadInstNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_DemandInstNum), $"{header.LE_DemandInstNum:X}");
        table.Rows.Add(nameof(LeHeader.LE_HeapExtra), $"{header.LE_HeapExtra:X}");

        return table;
    }

    private void MakeNames()
    {
        if (_manager.ResidentNames.Count == 0)
            goto __nonResident;
        ResidentNames.Columns.Add("Name");
        ResidentNames.Columns.Add("Ordinal");
        ResidentNames.Columns.Add("Size (hex)");
        foreach (ResidentName name in _manager.ResidentNames)
        {
            ResidentNames.Rows.Add(name.Name, name.Ordinal, name.Size.ToString("X"));
        }
        
        __nonResident:
        if (_manager.NonResidentNames.Count == 0)
            goto __imports;
        
        NonResidentNames.Columns.Add("Name");
        NonResidentNames.Columns.Add("Ordinal");
        NonResidentNames.Columns.Add("Size (hex)");
        foreach (NonResidentName name in _manager.NonResidentNames)
        {
            NonResidentNames.Rows.Add(name.Name, name.Ordinal, name.Size);
        }
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
        ObjectsTable.Columns.Add("Virtual Size");
        ObjectsTable.Columns.Add("Base Relocation");
        ObjectsTable.Columns.Add("Flags");
        ObjectsTable.Columns.Add("Map Index");
        ObjectsTable.Columns.Add("Map Entries");
        ObjectsTable.Columns.Add("Unknown");
        ObjectsTable.Columns.Add("Translated flags");
        
        foreach (ObjectTableModel table in _manager.ObjectTables)
        {
            string text = table
                .ObjectFlags
                .Aggregate("", (current, s) => current + $"`{s}` ");
            
            ObjectsTable.Rows.Add(
                table.ObjectTable.VirtualSegmentSize,
                table.ObjectTable.RelocationBaseAddress,
                table.ObjectTable.ObjectFlags,
                table.ObjectTable.PageMapIndex,
                table.ObjectTable.PageMapEntries,
                table.ObjectTable.Unknown,
                text
            );
        }

        ObjectPages.Columns.Add("High Page (hex)");
        ObjectPages.Columns.Add("Low Page (hex)");
        ObjectPages.Columns.Add("Flags (hex)");
        ObjectPages.Columns.Add("Real Offset (hex)");
        ObjectPages.Columns.Add("Translated flags");

        foreach (ObjectPageModel page in _manager.ObjectPages)
        {
            string flags = page
                .Flags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");

            ObjectPages.Rows.Add(
                page.Page.HighPage.ToString("X"),
                page.Page.LowPage.ToString("X"),
                page.Page.Flags.ToString("X"),
                page.RealOffset.ToString("X"), 
                flags
            );
        }
    }

    private void MakeEntryTable()
    {
        // prepare general Entry bundles table
        DataTable table = new("Bundles table (EntryPoint table main part)")
        {
            Columns = { "#", "Count", "Bundle#", "Object#", "Flags" }
        };
        List<DataTable> entriesForEachBundle = [];
        
        foreach (EntryBundleModel bundle in _manager.EntryBundles)
        {
            string flags = bundle
                .Flags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");
            
            table.Rows.Add(bundle.BundleNumber, bundle.EntryBundle.EntriesCount, bundle.EntryBundle.EntryBundleIndex, bundle.EntryBundle.ObjectIndex, flags);
            
            DataTable entries = new($"Bundle #{bundle.BundleNumber} | EntryTable")
            {
                Columns = { "Flag (hex)", "Offset (hex)", "FlagNames" }
            };
            
            if (bundle.EntryBundle.Entries.Length != 0)
            {
                entries.TableName = $"Bundle #{bundle.BundleNumber} | 16-bit Entries ";
                foreach (Entry16 entry in bundle.EntryBundle.Entries)
                {
                    string efl = bundle
                        .Flags
                        .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");
                    entries.Rows.Add(entry.Flag, entry.Offset.ToString("X"),  efl);
                }
            }
            else if (bundle.EntryBundle.ExtendedEntries.Length != 0)
            {
                entries.TableName = $"Bundle #{bundle.BundleNumber} | 32-bit Entries";
                foreach (Entry16 entry in bundle.EntryBundle.Entries)
                {
                    string efl = bundle
                        .Flags
                        .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");
                    entries.Rows.Add(entry.Flag, entry.Offset.ToString("X"),  efl);
                }
            }
            entriesForEachBundle.Add(entries);
        }

        EntryTables = [table, ..entriesForEachBundle];
        
    }


    private void MakeFixupTables()
    {
        FixupPages.Columns.Add("Index");
        FixupPages.Columns.Add("Position (hex)");

        for (int i = 0; i < _manager.FixupPagesOffsets.Count; i++)
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

        foreach (FixupRecordsTableModel record in _manager.FixupRecords)
        {
            rawTable.Rows.Add(
                record.Record.AddressType,
                record.Record.RelocationType,
                record.Record.TargetObject,
                record.Record.AddValue,
                record.Record.ExtraData,
                record.Record.ModuleIndex,
                record.Record.NameOffset,
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
                "OS/2 Fixup",
                "Ordinal",
                "Name",
                "Address Type",
                "Reloc. Type"
            }
        };
        
        foreach (FixupRecordsTableModel model in _manager.FixupRecords)
        {
            string atp = model.AddressTypeFlags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");
            string rtp = model.RecordTypeFlags
                .Aggregate(string.Empty, (current, s) => current + $"`{s}` ");

            table.Rows.Add(
                model.PageIndex,
                model.Record.TargetObject,
                model.Record.AddValue,
                model.Record.ExtraData,
                model.Record.OsFixup,
                model.ImportingOrdinal,
                model.ImportingName,
                atp,
                rtp
                );
        }

        FixupRecords = [rawTable, table];
    }

    private void MakeCharacteristics()
    {
        Characteristics.Add("### Program Header information");
        Characteristics.Add("Target CPU: " + GetCpuType(_manager.LeHeader.LE_CPU));
        Characteristics.Add("Target OS: " + GetOsType(_manager.LeHeader.LE_OS));
        
        Characteristics.Add("Version and module flags set in Program header: 0x" + _manager.LeHeader.LE_Version.ToString("X"));
        Characteristics.AddRange(GetModuleFlags(_manager.LeHeader.LE_Type));
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
            _ => $"Unknown CPU (0x{cpuType:X4})"
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
}