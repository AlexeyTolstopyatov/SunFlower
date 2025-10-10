using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using Sunflower.Mz.Models;

namespace Sunflower.Mz.Services;

public class MzRelocationsVisualizer(List<MzRelocation> @struct) : AbstractStructVisualizer<List<MzRelocation>>(@struct)
{
    public override DataTable ToDataTable()
    {
        var dt = new DataTable()
        {
            Columns = { "Segment:2", "Offset:2" }
        };
        foreach (var relocation in _struct)
        {
            dt.Rows.Add($"0x{relocation.Segment:X4}", $"0x{relocation.Offset:X4}");
        }

        return dt;
    }

    public override string ToString()
    {
        return @"DOS executable relocation records
This region contains translated FAR relocations already. 
FAR-pointers are stores like 16:16-formated CPU WORD (offset:segment) 
_using little endian reinterpretation_";
    }

    public override Region ToRegion()
    {
        return new Region("## DOS Executable Relocations", ToString(), ToDataTable());
    }
}