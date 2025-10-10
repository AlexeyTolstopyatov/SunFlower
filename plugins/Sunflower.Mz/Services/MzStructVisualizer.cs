using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace Sunflower.Mz.Services;

public class MzStructVisualizer(MzHeader @struct) : AbstractStructVisualizer<MzHeader>(@struct)
{
    public override DataTable ToDataTable()
    {
        var table = new DataTable()
        {
            Columns = { "Segment", "Value" }
        };
        table.Rows.Add(nameof(_struct.e_sign), "0x" + _struct.e_sign.ToString("X"));
        table.Rows.Add(nameof(_struct.e_cblp), "0x" + _struct.e_cblp.ToString("X"));
        table.Rows.Add(nameof(_struct.e_cp), "0x" + _struct.e_cp.ToString("X"));
        table.Rows.Add(nameof(_struct.e_relc), "0x" + _struct.e_relc.ToString("X"));
        table.Rows.Add(nameof(_struct.e_pars), "0x" + _struct.e_pars.ToString("X"));
        table.Rows.Add(nameof(_struct.e_minep), "0x" + _struct.e_minep.ToString("X"));
        table.Rows.Add(nameof(_struct.e_maxep), "0x" + _struct.e_maxep.ToString("X"));
        table.Rows.Add(nameof(_struct.ss), "0x" + _struct.ss.ToString("X"));
        table.Rows.Add(nameof(_struct.sp), "0x" + _struct.sp.ToString("X"));
        table.Rows.Add(nameof(_struct.e_check), "0x" + _struct.e_check.ToString("X"));
        table.Rows.Add(nameof(_struct.ip), "0x" + _struct.ip.ToString("X"));
        table.Rows.Add(nameof(_struct.cs), "0x" + _struct.cs.ToString("X"));
        table.Rows.Add(nameof(_struct.e_lfarlc), "0x" + _struct.e_lfarlc.ToString("X"));
        table.Rows.Add(nameof(_struct.e_ovno), "0x" + _struct.e_ovno.ToString("X"));
        table.Rows.Add(nameof(_struct.e_oemid), "0x" + _struct.e_oemid.ToString("X"));
        table.Rows.Add(nameof(_struct.e_oeminfo), "0x" + _struct.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(_struct.e_lfanew), "0x" + _struct.e_lfanew.ToString("X"));

        return table;
    }

    public override string ToString()
    {
        return @"Main data structure for PC-DOS 2.0+, MS-DOS 2.0+ programs, 
which stores initial expected values and requirements for running";
    }

    public override Region ToRegion()
    {
        return new Region("## DOS Executable Header", ToString(), ToDataTable());
    }
}