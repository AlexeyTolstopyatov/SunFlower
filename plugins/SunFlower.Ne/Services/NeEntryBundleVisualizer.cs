using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeEntryBundleVisualizer(NeEntryBundle bundle, int number) : AbstractStructVisualizer<NeEntryBundle>(bundle)
{
    public override DataTable ToDataTable()
    {
        DataTable entries = new($"EntryTable Bundle #{number}")
        {
            Columns =
            {
                FlowerReport.ForColumn("Ordinal", typeof(ushort)),
                FlowerReport.ForColumn("Offset", typeof(ushort)),
                FlowerReport.ForColumn("Segment", typeof(ushort)),
                FlowerReport.ForColumn("Entry", typeof(string)),
                FlowerReport.ForColumn("Data", typeof(string)),
                FlowerReport.ForColumn("Type", typeof(string))
            }
        };
        foreach (var item in bundle.EntryPoints)
        {
            entries.Rows.Add(
                "@" + item.Ordinal,
                item.Offset.ToString("X"),
                item.Segment,
                item.Entry,
                item.Data,
                item.Type
            );
        }

        return entries;
    }

    public override Region ToRegion()
    {
        throw new NotImplementedException();
    }
}