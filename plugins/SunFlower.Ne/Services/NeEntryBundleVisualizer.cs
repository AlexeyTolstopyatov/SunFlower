using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeEntryBundleVisualizer(NeEntryBundle @struct, int number) : AbstractStructVisualizer<NeEntryBundle>(@struct)
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
        foreach (var item in _struct.EntryPoints)
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

    public override string ToString()
    {
        return @"Bundle contains EntryPoints what are the same with one of characteristics
Loader of segmented executables reads entry points bundle-by bundle
if bundle marked as unused, loader skips count of entries set in this bundle.";
    }

    public override Region ToRegion()
    {
        return new Region($"### EntryTable Bundle #{number}", ToString(), ToDataTable());
    }
}