namespace SunFlower.Le.Services;

public class LxObjectsManager
{
    public List<Headers.Le.Object> Objects { get; set; } = [];

    public LxObjectsManager(BinaryReader reader, uint offset, uint count)
    {
        reader.BaseStream.Position = offset;
        
        for (var i = 0; i < count; i++)
        {
            var virtualSegmentSize = reader.ReadUInt32();
            var relocationBase = reader.ReadUInt32();
            var objectFlagsMask = reader.ReadUInt32();
            var pageMap = reader.ReadUInt32();
            var pageMapEntries = reader.ReadUInt32();
            var unknownField = reader.ReadUInt32();
            
            Objects.Add(new Headers.Le.Object()
            {
                VirtualSegmentSize = virtualSegmentSize,
                ObjectFlagsMask = objectFlagsMask,
                PageMapEntries = pageMapEntries,
                PageMapIndex = pageMap,
                RelocationBaseAddress = relocationBase,
                Unknown = unknownField
            });
        }
    }
}