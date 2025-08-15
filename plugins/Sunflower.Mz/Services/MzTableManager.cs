using System.Data;
using SunFlower.Abstractions.Types;

namespace Sunflower.Mz.Services;

public class MzTableManager
{
    public List<Region> Regions { get; private set; }
    
    public MzTableManager(MzDumpManager dumpManager)
    {
        var list = new List<Region>
        {
            MakeDosHeader(dumpManager.Header),
            MakeRelocations(dumpManager.Relocations),
            MakeSeedDetails(dumpManager.Header)
        };

        Regions = list;
    }

    private static Region MakeDosHeader(MzHeader dosHeader)
    {
        var head = "### Mark Zbikowski MS/PC-DOS Executable Header (16-bit)";
        var content = "Main data structure for PC-DOS 2.0+, MS-DOS 2.0+ programs, which stores initial expected FAR values and binary technical details";
        var table = new DataTable()
        {
            Columns = { "Segment", "Value" }
        };
        table.Rows.Add(nameof(dosHeader.e_sign), "0x" + dosHeader.e_sign.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_cblp), "0x" + dosHeader.e_cblp.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_cp), "0x" + dosHeader.e_cp.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_relc), "0x" + dosHeader.e_relc.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_pars), "0x" + dosHeader.e_pars.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_minep), "0x" + dosHeader.e_minep.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_maxep), "0x" + dosHeader.e_maxep.ToString("X"));
        table.Rows.Add(nameof(dosHeader.ss), "0x" + dosHeader.ss.ToString("X"));
        table.Rows.Add(nameof(dosHeader.sp), "0x" + dosHeader.sp.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_check), "0x" + dosHeader.e_check.ToString("X"));
        table.Rows.Add(nameof(dosHeader.ip), "0x" + dosHeader.ip.ToString("X"));
        table.Rows.Add(nameof(dosHeader.cs), "0x" + dosHeader.cs.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_lfarlc), "0x" + dosHeader.e_lfarlc.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_ovno), "0x" + dosHeader.e_ovno.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_oemid), "0x" + dosHeader.e_oemid.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_oeminfo), "0x" + dosHeader.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(dosHeader.e_lfanew), "0x" + dosHeader.e_lfanew.ToString("X"));
        
        return new Region(head, content, table);
    }
    
    private static Region MakeRelocations(List<uint> rels)
    {
        var head = "### Relocations";
        var content = "This region contains translated FAR relocations already. FAR-pointers are stores like 16:16-formated CPU WORD (offset:segment)";
        var table = new DataTable()
        {
            Columns = { "#", "RawBytes:4", "16:16" }
        };

        for (int i = 0; i < rels.Count; ++i)
        {
            var segment = rels[i] >> 16;
            var offset = rels[i] & 0xFFFF;
            table.Rows.Add((i + 1), $"{rels[i]:X8}", $"{segment:X4}:{offset:X4}");
        }
        
        return new Region(head, content, table);
    }

    private static Region MakeSeedDetails(MzHeader header)
    {
        var head = "### FlowerSeed Technical details region";
        var content = "This region contains translated technical details from other parts of current executable.";

        var table = new DataTable()
        {
            Columns = { "Target", "Value" }
        };

        var ovnoText = (header.e_ovno) switch
        {
            0 => "First and Main executable binary.",
            _ => $"Overlay part #{header.e_ovno} of current Executable"
        };
        var reservedAt1C = (header.e_res0x1c.Any(x => x == 0)) switch
        {
            true => "Reserved BYTE-array has right values. (always zero)",
            false => "Take a look at BYTE-array at 0x1C absolute offset. Expected LINK.EXE/DEBUG.EXE/malware information bytes"
        };
        var reservedAt28 = (header.e_res_0x28.Any(x => x == 0)) switch
        {
            true => "Reserved BYTE-array has right values. (always zero)",
            false => "Take a look at BYTE-array at 0x28 absoulte offset. It can be malware rewritten bytes."
        };
        var lfanewText = (header.e_lfanew != 0) switch
        {
            true => $"Image has NEAR pointer `e_lfanew={header.e_lfanew:X8}` to next data structure.",
            false => "Clear MS/PC-DOS image."
        };
        table.Rows.Add("`e_ovno`", ovnoText);
        table.Rows.Add("`e_res0x1C`", reservedAt1C);
        table.Rows.Add("`e_res0x28`", reservedAt28);
        table.Rows.Add("`e_lfanew`", lfanewText);
        return new(head, content, table);
    }
}