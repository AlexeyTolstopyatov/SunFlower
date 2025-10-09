using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace SunFlower.Ne.Services;

public class NeModuleReferencesVisualizer(List<ushort> @struct) : AbstractStructVisualizer<List<ushort>>(@struct)
{
    public override DataTable ToDataTable()
    {
        DataTable modres = new("Module References")
        {
            Columns =
            {
                FlowerReport.ForColumn("Reference#:2", typeof(int)), 
                FlowerReport.ForColumn("Offset:2", typeof(ushort))
            }
        };
        for (var i = 0; i < _struct.Count; ++i)
        {
            modres.Rows.Add(
                i + 1,
                $"0x{_struct[i]:X4}"
            );
        }

        return modres;
    }

    public override string ToString()
    {
        return new FlowerDescriptor()
            .Line("The module-reference table follows the resident-name table. ")
            .Line("Each entry contains an offset for the module-name string within the imported names table; ")
            .Line("each entry is 2 bytes long.")
            .ToString();
    }

    public override Region ToRegion()
    {
        return new Region("### Module Refernces Table", ToString(), ToDataTable());
    }
}