namespace SunFlower.Le.Headers.Le;

public static class FixupRelocationAddressType
{
    public static string GetAddressType(byte atp)
    {
        return (atp >> 4) switch
        {
            0 => "ADDR_LOW_BYTE",
            2 => "ADDR_SELECTOR_16_BIT",
            3 => "ADDR_FAR_PTR_32",
            5 => "ADDR_OFFSET_16",
            6 => "ADDR_FAR_PTR_48",
            7 => "ADDR_OFFSET_32",
            8 => "ADDR_OFFSET_RELEIP_32",
            _ => "ADDR_RESERVED"
        };
    }

    public static bool HasOffsetList(byte atp) => (atp & 0x02) != 0;
    public static bool Is16BitAlias(byte atp) => (atp & 0x01) != 0;
}