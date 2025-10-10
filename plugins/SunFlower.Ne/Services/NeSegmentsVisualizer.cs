using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeSegmentsVisualizer(List<SegmentModel> @struct) : AbstractStructVisualizer<List<SegmentModel>>(@struct)
{
    public override DataTable ToDataTable()
    {
        DataTable segs = new("Segments Table")
        {
            Columns =
            {
                FlowerReport.ForColumn("Type", typeof(string)),
                FlowerReport.ForColumn("#Segment", typeof(uint)),
                FlowerReport.ForColumn("Offset", typeof(ushort)),
                FlowerReport.ForColumn("Length", typeof(ushort)),
                FlowerReport.ForColumn("Flags", typeof(ushort)),
                FlowerReport.ForColumn("Minimum allocation", typeof(ushort)),
                FlowerReport.ForColumn("Flags", typeof(string))
            }
        };
        
        foreach (var segmentDump in _struct)
        {
            var array = segmentDump
                .Characteristics
                .Aggregate("", (current, characteristic) => current + characteristic + " ");
            
            segs.Rows.Add(
                segmentDump.Type,
                "0x" + segmentDump.SegmentNumber.ToString("X"),
                "0x" + segmentDump.FileOffset.ToString("X"),
                "0x" + segmentDump.FileLength.ToString("X"),
                "0x" + segmentDump.Flags.ToString("X"),
                "0x" + segmentDump.MinAllocation.ToString("X"),
                array
            );
        }

        return segs;
    }

    public override string ToString()
    {
        return @"The segment table contains an entry for each segment in the executable file.
            The number of segment table entries are defined in the segmented EXE header
            The first entry in the segment table is segment number 1
            The following is the structure of a segment table entry. ";
    }

    public override Region ToRegion()
    {
        return new Region("## Segments", ToString(), ToDataTable());
    }
}