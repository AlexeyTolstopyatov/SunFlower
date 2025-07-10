namespace SunFlower.Le.Headers.Le;

public static class FixupRelocationType
{
    public static string GetRelocationType(byte rtp)
    {
        return (rtp >> 6) switch
        {
            0 => "REL_INTERNAL_REF",
            1 => "REL_IMPORT_ORD",
            2 => "REL_IMPORT_NAME",
            3 => "REL_OSFIXUP",
            _ => "REL_INVALID"
        };
    }

    public static bool IsAdditive(byte rtp) => (rtp & 0x01) != 0;
    public static bool Is32BitTarget(byte rtp) => (rtp & 0x02) != 0;
    public static bool Is16BitOrdinal(byte rtp) => (rtp & 0x08) != 0;
    public static bool HasExtraData(byte rtp) => (rtp & 0x10) != 0;
}