using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeSegmentRelocationsVisualization(SegmentModel @struct) : AbstractStructVisualizer<SegmentModel>(@struct)
{
    public override DataTable ToDataTable()
    {
        if (_struct.Relocations.Count == 0)
            return new DataTable();
        
        DataTable table = new()
        {
            Columns =
            {
                FlowerReport.ForColumn("ATP", typeof(byte)),
                FlowerReport.ForColumn("RTP", typeof(byte)),
                FlowerReport.ForColumn("RTP", typeof(string)),
                FlowerReport.ForColumn("Additive?", typeof(bool)),
                FlowerReport.ForColumn("OffsetInSeg", typeof(ushort)),
                FlowerReport.ForColumn("SegType", typeof(ushort)),
                FlowerReport.ForColumn("Target", typeof(ushort)),
                FlowerReport.ForColumn("TargetType", typeof(string)),
                FlowerReport.ForColumn("Mod#", typeof(string)),
                FlowerReport.ForColumn("Name", typeof(ushort)),
                FlowerReport.ForColumn("Ordinal", typeof(ushort)),
                FlowerReport.ForColumn("Fixup", typeof(string))
            }
        };
        
        foreach (var rel in _struct.Relocations)
        {
            // prepare table
            table.Rows.Add(
                "0x" + rel.AddressType.ToString("X"), 
                "0x" + rel.RelocationFlags.ToString("X"), 
                $"[{rel.RelocationType}]", 
                $"[{rel.IsAdditive}]",
                "0x" + rel.OffsetInSegment.ToString("X"), 
                $"{rel.SegmentType}", 
                "0x" + rel.Target.ToString("X"), 
                $"[{rel.TargetType}]", 
                "0x" + rel.ModuleIndex.ToString("X"),
                "0x" + rel.NameOffset.ToString("X"),
                "@" + rel.Ordinal, 
                rel.Fixup);
        }

        return table;
    }

    public override string ToString()
    {
        return (_struct.Relocations.Count == 0) switch
        {
            true => "Doesn't have own relocations table",
            false => @"The location and size of the per-segment data is defined in the segment table entry for the segment.
If the segment has relocation fixups, as defined in the segment table entry flags, they directly
follow the segment data in the file."
        };
    }

    public override Region ToRegion()
    {
        return new Region($"### Segment #{_struct.SegmentNumber}", ToString(), ToDataTable());
    }
}