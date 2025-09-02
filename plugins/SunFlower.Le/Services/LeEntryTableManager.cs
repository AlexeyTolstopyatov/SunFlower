using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Services;

public class LeEntryTableManager
{
    public List<EntryBundle> EntryBundles { get; set; } = [];

    public LeEntryTableManager(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Position = offset;
        byte bundleSize;
        var currentOrdinal = 1; // global exports iterator.
        
        while ((bundleSize = reader.ReadByte()) != 0)
        {
            var bundleFlags = reader.ReadByte(); 
            var objectIndex = reader.ReadUInt16();

            EntryBundle bundle = new(bundleSize, bundleFlags, objectIndex);

            var is32Bit = (bundleFlags & 0b00000010) != 0;

            List<Entry> entries = [];
            for (var i = 0; i < bundleSize; i++)
            {
                var entryFlags = reader.ReadByte();
                var entryOffset = is32Bit ? reader.ReadUInt32() : reader.ReadUInt16();
                
                entries.Add(new Entry(currentOrdinal, entryFlags, entryOffset));
                currentOrdinal++;
            }

            bundle.Entries = entries.ToArray();
            
            EntryBundles.Add(bundle);
        }
    }
}