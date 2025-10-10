using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace Sunflower.Mz.Services;

public class MzDetailsVisualizer(MzHeader @struct) : AbstractStructVisualizer<MzHeader>(@struct)
{
    public override DataTable ToDataTable()
    {
        var table = new DataTable()
        {
            Columns = { "Target", "Value" }
        };

        var ovnoText = _struct.e_ovno switch
        {
            0 => "Main executable binary.",
            _ => $"Overlay part #{_struct.e_ovno} of current Executable"
        };
        var reservedAt1C = _struct.e_res0x1c.Any(x => x == 0) switch
        {
            true => "Reserved BYTE-array has right values. (always zero)",
            false => "Take a look at BYTE-array at 0x1C absolute offset. Expected LINK.EXE/DEBUG.EXE/malware information bytes"
        };
        var reservedAt28 = _struct.e_res_0x28.Any(x => x == 0) switch
        {
            true => "Reserved BYTE-array has right values. (always zero)",
            false => "Take a look at BYTE-array at 0x28 absoulte offset. It can be malware rewritten bytes."
        };
        var lfarlc = (_struct.e_lfarlc == 0x40) switch
        {
            true => "Look for next headers signature. It may be protected mode executable",
            false => ""
        };
        var lfanewText = (_struct.e_lfanew != 0) switch
        {
            true => $"Image has NEAR pointer `e_lfanew=0x{_struct.e_lfanew:X8}` to next data structure.",
            false => "Clear MS/PC-DOS image."
        };
        table.Rows.Add("`e_ovno`", ovnoText);
        table.Rows.Add("`e_res0x1C`", reservedAt1C);
        table.Rows.Add("`e_res0x28`", reservedAt28);
        table.Rows.Add("`e_lfanew`", lfanewText);
        table.Rows.Add("`e_lfarlc`", lfarlc);

        return table;
    }

    public override string ToString()
    {
        return @"This section describes details of real-mode DOS executable";
    }

    public override Region ToRegion()
    {
        return new Region("### Details", ToString(), ToDataTable());
    }
}