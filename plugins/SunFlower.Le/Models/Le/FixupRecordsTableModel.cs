using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Models.Le;

public class FixupRecordsTableModel(int pageIndex, FixupRecord rec, List<string> atp, List<string> rtp, string name, string ordinal)
{
    public int PageIndex { get; } = pageIndex;
    public FixupRecord Record { get; } = rec;
    public string[] AddressTypeFlags { get; } = atp.ToArray();
    public string[] RecordTypeFlags { get; } = rtp.ToArray();
    public string ImportingName { get; } = name;
    public string ImportingOrdinal { get; } = ordinal;
}