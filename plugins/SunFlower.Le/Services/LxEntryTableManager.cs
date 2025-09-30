using SunFlower.Le.Headers.Lx;

namespace SunFlower.Le.Services;

public class LxEntryTableManager(BinaryReader reader, uint offset)
{
    public List<EntryBundle> EntryBundles => ReadEntryTable(reader, offset);

    private static List<EntryBundle> ReadEntryTable(BinaryReader reader, uint entryTableOffset)
    {
        reader.BaseStream.Position = entryTableOffset;
        var bundles = new List<EntryBundle>();
        
        while (true)
        {
            var count = reader.ReadByte();
            if (count == 0) break;

            var typeValue = reader.ReadByte();
            var type = (EntryBundleType)(typeValue & 0x7F);
            
            var bundle = new EntryBundle { Count = count, Type = type };

            for (var i = 0; i < count; i++)
            {
                Entry entry;
                switch (type)
                {
                    case EntryBundleType.Unused:
                        reader.ReadByte();
                        entry = new EntryUnused();
                        break;
                    case EntryBundleType._16Bit:
                        entry = new Entry16Bit
                        {
                            ObjectNumber = reader.ReadUInt16(),
                            Flags = reader.ReadByte(),
                            Offset = reader.ReadUInt16()
                        };
                        break;
                    case EntryBundleType._286CallGate:
                        entry = new Entry286CallGate
                        {
                            ObjectNumber = reader.ReadUInt16(),
                            Flags = reader.ReadByte(),
                            Offset = reader.ReadUInt16(),
                            CallGateSelector = reader.ReadUInt16()
                        };
                        break;
                    case EntryBundleType._32Bit:
                        entry = new Entry32Bit
                        {
                            ObjectNumber = reader.ReadUInt16(),
                            Flags = reader.ReadByte(),
                            Offset = reader.ReadUInt32()
                        };
                        break;
                    case EntryBundleType.Forwarder:
                        reader.ReadUInt16(); // skip wReserved
                        entry = new EntryForwarder
                        {
                            Flags = reader.ReadByte(),
                            ModuleOrdinal = reader.ReadUInt16(),
                            OffsetOrOrdinal = reader.ReadUInt32()
                        };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }

                bundle.Entries.Add(entry);
            }
            bundles.Add(bundle);
        }
        return bundles;
    }
}