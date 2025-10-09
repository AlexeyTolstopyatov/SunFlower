using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Services;

public class MzStructVisualizer(MzHeader @struct) : AbstractStructVisualizer<MzHeader>(@struct)
{
    public override DataTable ToDataTable()
    {
        var mz = _struct;
        DataTable table = new()
        {
            TableName = "DOS/2 Extended Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.e_sign), mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_cblp), mz.e_cblp.ToString("X"));
        table.Rows.Add(nameof(mz.e_cp), mz.e_cp.ToString("X"));
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

    public override string ToString()
    {
        return new FlowerDescriptor()
            .Line("Intel 8086 real-mode DOS-executables")
            .Line("always have this header for correct resources allocation.")
            .Line("Pointer of relocation's table set at the ").Inline("0x40")
            .ToString();
    }

    public override Region ToRegion()
    {
        return new Region("## DOS 2.0+ Executable Header", ToString(), ToDataTable());
    }
}