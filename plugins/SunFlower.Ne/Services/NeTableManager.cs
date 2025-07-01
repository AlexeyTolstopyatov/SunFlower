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
        MakeNonResidentNames();
    }
    
    public DataTable[] Headers { get; set; } = [];
    public DataTable SegmentTable { get; set; } = new();
    public DataTable ResidentNamesTable { get; set; } = new();
    public DataTable NonResidentNamesTable { get; set; } = new();
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
        table.Rows.Add(nameof(ne.magic), ne.magic.ToString("X"));
        table.Rows.Add(nameof(ne.ver), ne.ver.ToString("X"));
        table.Rows.Add(nameof(ne.rev), ne.rev.ToString("X"));
        table.Rows.Add(nameof(ne.enttab), ne.enttab.ToString("X"));
        table.Rows.Add(nameof(ne.cbenttab), ne.cbenttab.ToString("X"));
        table.Rows.Add(nameof(ne.crc), ne.crc.ToString("X"));
        table.Rows.Add(nameof(ne.pflags), ne.pflags.ToString("X"));
        table.Rows.Add(nameof(ne.aflags), ne.aflags.ToString("X"));
        table.Rows.Add(nameof(ne.autodata), ne.autodata.ToString("X"));
        table.Rows.Add(nameof(ne.heap), ne.heap.ToString("X"));
        table.Rows.Add(nameof(ne.stack), ne.stack.ToString("X"));
        table.Rows.Add(nameof(ne.csip), ne.csip.ToString("X"));
        table.Rows.Add(nameof(ne.sssp), ne.sssp.ToString("X"));
        table.Rows.Add(nameof(ne.cseg), ne.cseg.ToString("X"));
        table.Rows.Add(nameof(ne.cmod), ne.cmod.ToString("X"));
        table.Rows.Add(nameof(ne.segtab), ne.segtab.ToString("X"));
        table.Rows.Add(nameof(ne.rsrctab), ne.rsrctab.ToString("X"));
        table.Rows.Add(nameof(ne.restab), ne.restab.ToString("X"));
        table.Rows.Add(nameof(ne.cmovent), ne.cmovent.ToString("X"));
        table.Rows.Add(nameof(ne.align), ne.align.ToString("X"));
        table.Rows.Add(nameof(ne.os), ne.os.ToString("X"));
        table.Rows.Add(nameof(ne.flagsothers), ne.flagsothers.ToString("X"));
        table.Rows.Add(nameof(ne.pretthunks), ne.pretthunks.ToString("X"));
        table.Rows.Add(nameof(ne.psegrefbytes), ne.psegrefbytes.ToString("X"));
        table.Rows.Add(nameof(ne.swaparea), ne.swaparea.ToString("X"));
        table.Rows.Add(nameof(ne.minor), ne.minor.ToString("X"));
        table.Rows.Add(nameof(ne.magic), ne.major.ToString("X"));

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
            Columns = { "Offset", "Segment", "Entry", "Data type", "Entry type" }
        };
        
        foreach (NeEntryTableModel item in _manager.EntryTableItems)
        {
            entries.Rows.Add(
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
        for (Int32 i = 0; i < _manager.ModuleReferences.Length; ++i)
        {
            modres.Rows.Add(
                i + 1,
                _manager.ModuleReferences[i].ImportOffset
            );
        }

        ModuleReferencesTable = modres;
    }

    private void MakeNonResidentNames()
    {
        if (_manager.ExportingFunctions.Length == 0)
            return;
        
        DataTable nonres = new("Non Resident Names table")
        {
            Columns = { "Count", "Name", "Ordinal" }
        };
        foreach (NeExport neExportDump in _manager.ExportingFunctions)
        {
            nonres.Rows.Add(
                neExportDump.Count,
                neExportDump.Name,
                "@" + neExportDump.Ordinal
            );
        }

        NonResidentNamesTable = nonres;
    }

    private void MakeCharacteristics()
    {
        List<string> md = [];
        md.Add("### Image");

        string os = _manager.NeHeader.os switch
        {
            0x1 => "OS/2",
            0x2 => "Win16",
            0x3 => "DOS/4",
            0x4 => "Win32s",
            0x5 => "BoSS",
            _ => "Not specified"
        };

        string cpu = _manager.NeHeader.pflags switch
        {
            var f when (f & 0x4) != 0 => "I8086",
            var f when (f & 0x5) != 0 => "I286",
            var f when (f & 0x6) != 0 => "I386",
            var f when (f & 0x7) != 0 => "I8087",
            _ => "Not specified"
        };
        md.Add($"Target operating system environment: `{os}`");
        md.Add($"Target CPU architecture: `{cpu}`");
        
        Characteristics = md.ToArray();
    }
}