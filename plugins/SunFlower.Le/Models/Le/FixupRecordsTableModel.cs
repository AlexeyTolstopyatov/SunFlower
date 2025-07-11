using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models.Le;

public class FixupRecordsTableModel(int pageIndex, FixupRecord rec, List<string> atp, List<string> rtp, string name, string ordinal)
{
    public int PageIndex { get; set; } = pageIndex;
    public FixupRecord Record { get; set; } = rec;
    public string[] AddressTypeFlags { get; set; } = atp.ToArray();
    public string[] RecordTypeFlags { get; set; } = rtp.ToArray();
    public string ImportingName { get; set; } = name;
    public string ImportingOrdinal { get; set; } = ordinal;
}