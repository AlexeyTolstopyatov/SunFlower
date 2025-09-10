using System.Runtime.InteropServices;
using SunFlower.Le.Headers.Lx;

namespace SunFlower.Le.Services;

public class LxEntryTableManager(BinaryReader reader, uint offset)
{
    public List<EntryBundle> EntryBundles { get; init; } = ReadEntryTable(reader, offset);

    private static List<EntryBundle> ReadEntryTable(BinaryReader reader, uint entryTableOffset, [Optional] uint bundlesCount)
    {
        reader.BaseStream.Seek(entryTableOffset, SeekOrigin.Begin);
        var bundles = new List<EntryBundle>();
        
        while (true)
        {
            var count = reader.ReadByte();
            if (count == 0) break;

            var typeValue = reader.ReadByte();
            var type = (EntryBundleType)(typeValue & 0x7F);
            var hasParamTypes = (typeValue & 0x80) != 0;
            
            
            var bundle = new EntryBundle() { Count = count, Type = type };

            for (var i = 0; i < count; i++)
            {
                Entry entry = type switch
                {
                    EntryBundleType.Unused => new EntryUnused(),
                    EntryBundleType._16Bit => new Entry16Bit
                    {
                        ObjectNumber = reader.ReadUInt16(),
                        Flags = reader.ReadByte(),
                        Offset = reader.ReadUInt16()
                    },
                    EntryBundleType._32Bit => new Entry32Bit
                    {
                        ObjectNumber = reader.ReadUInt16(),
                        Flags = reader.ReadByte(),
                        Offset = reader.ReadUInt32()
                    },
                    EntryBundleType._286CallGate => new Entry286CallGate
                    {
                        ObjectNumber = reader.ReadUInt16(),
                        Flags = reader.ReadByte(),
                        Offset = reader.ReadUInt16(),
                        CallGateSelector = reader.ReadUInt16()
                    },
                    EntryBundleType.Forwarder => new EntryForwarder
                    {
                        Flags = reader.ReadByte(),
                        ModuleOrdinal = reader.ReadUInt16(),
                        OffsetOrOrdinal = reader.ReadUInt32()
                    },
                    _ => new EntryUnused()
                };
                bundle.Entries.Add(entry);
            }
            bundles.Add(bundle);
        }
        return bundles;
    }
}