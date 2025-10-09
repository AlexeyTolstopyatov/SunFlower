using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class MzStructVisualizer(MzHeader @struct) : AbstractStructVisualizer<MzHeader>(@struct)
{
    private readonly MzHeader _struct = @struct;
    private readonly string _content = "### DOS 2.0+ Executable Header";
    public override DataTable ToDataTable()
    {
        var mz = _struct;
        DataTable table = new()
        {
            TableName = "DOS/2 Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.e_sign), "0x" + mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_cblp), "0x" + mz.e_cblp.ToString("X"));
        table.Rows.Add(nameof(mz.e_lb), "0x" + mz.e_lb.ToString("X"));
        table.Rows.Add(nameof(mz.e_relc), "0x" + mz.e_relc.ToString("X"));
        table.Rows.Add(nameof(mz.e_pars), "0x" + mz.e_pars.ToString("X"));
        table.Rows.Add(nameof(mz.e_minep), "0x" + mz.e_minep.ToString("X"));
        table.Rows.Add(nameof(mz.e_maxep), "0x" + mz.e_maxep.ToString("X"));
        table.Rows.Add(nameof(mz.ss), "0x" + mz.ss.ToString("X"));
        table.Rows.Add(nameof(mz.sp), "0x" + mz.sp.ToString("X"));
        table.Rows.Add(nameof(mz.e_crc), "0x" + mz.e_crc.ToString("X"));
        table.Rows.Add(nameof(mz.ip), "0x" + mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), "0x" + mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfarlc), "0x" + mz.e_lfarlc.ToString("X"));
        table.Rows.Add(nameof(mz.e_ovno), "0x" + mz.e_ovno.ToString("X"));
        table.Rows.Add(nameof(mz.e_oemid), "0x" + mz.e_oemid.ToString("X"));
        table.Rows.Add(nameof(mz.e_oeminfo), "0x" + mz.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfanew), "0x" + mz.e_lfanew.ToString("X"));
        
        return table;
    }

    public override string ToString()
    {
        var content = new StringBuilder();
        content.AppendLine("Every PE32/+ linked executable starts from old `MZ` header.");
        content.AppendLine("The `e_lfarlc` pointer always set as `0x40` absolute offset");
        content.AppendLine("The DOS header and DOS stub till next COFF header are unused");
        return content.ToString();
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}