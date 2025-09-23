using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeEntryTableManager(BinaryReader reader, uint offset, uint bundlesCount)
{
    /// <summary>
    /// Exporting Addresses Table. I don't know what actually is that.
    /// </summary>
    public List<NeEntryBundle> EntryBundles { get; } = FindEntryBundles(reader, offset, bundlesCount);

    private static List<NeEntryBundle> FindEntryBundles(BinaryReader reader, uint bundlesOffset, uint bundlesCount)
    {
        reader.BaseStream.Position = bundlesOffset;
        
        var bundles = new List<NeEntryBundle>();
        var bytesRemaining = (int)bundlesCount;
        var currentOrdinal = 1;
        
        while (bytesRemaining > 0)
        {
            var count = reader.ReadByte();
            bytesRemaining--;

            if (count == 0) // <-- end
                break;

            var segId = reader.ReadByte();
            bytesRemaining--;

            var entries = new List<EntryTableModel>();

            if (segId == 0) // !UNuSED
            {
                for (var i = 0; i < count; i++)
                {
                    entries.Add(new EntryTableModel(true, false, 0)
                    {
                        Ordinal = (ushort)currentOrdinal++,
                        Segment = segId,
                    });
                }
            }
            else // CONST or MOVEABLE
            {
                var isMoveable = segId == 0xFF;
                var entrySize = isMoveable ? 6 : 3;
                var bundleDataSize = count * entrySize;

                if (bundleDataSize > bytesRemaining)
                    throw new InvalidDataException(
                        $"Inexact length: expected {bundleDataSize} bytes, got {bytesRemaining}");

                for (var i = 0; i < count; i++)
                {
                    if (isMoveable)
                    {
                        var flags = reader.ReadByte();
                        var magic = reader.ReadBytes(2);
                        var segment = reader.ReadByte();
                        var offset = reader.ReadUInt16();

                        entries.Add(new EntryTableModel(
                            false, true ,flags)
                        {
                            Offset = offset,
                            Segment = segment,
                            Ordinal = (ushort)currentOrdinal
                        });
                    }
                    else
                    {
                        var flags = reader.ReadByte();
                        var offset = reader.ReadUInt16();

                        entries.Add(new EntryTableModel(
                            false, false, flags)
                        {
                            Offset = offset,
                            Segment = segId,
                            Ordinal = (ushort)currentOrdinal,
                        });
                    }

                    currentOrdinal++;
                }

                bytesRemaining -= bundleDataSize;
            }

            bundles.Add(new NeEntryBundle()
            {
                EntryPoints = entries
            });
        }

        return bundles;
    }
}