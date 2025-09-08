using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeVbRuntime56Manager : DirectoryManager
{
    public Vb5Header Vb5Header { get; }
    private readonly FileSectionsInfo _info;
    private readonly BinaryReader _reader;

    public PeVbRuntime56Manager(FileSectionsInfo info, BinaryReader reader) : base(info)
    {
        _info = info;
        _reader = reader;

        Vb5Header = FindVb5Runtime();
    }

    private Vb5Header FindVb5Runtime()
    {
        // determine real offset to EntryPoint
        var header = new Vb5Header();
        
        try
        {
            var offset = Offset(_info.EntryPoint);
            _reader.BaseStream.Position = offset;

            var pushOpcode = _reader.ReadByte();
            var pushAddress = _reader.ReadUInt32(); // always 32-bit register.
            var callOpCode = _reader.ReadByte();
            var callProcedure = _reader.ReadUInt32(); // always 32-bit register too.
                                                         // VB supports only 16/32-bit code
            // push expression equals 0x68
            if (pushOpcode != 0x68)
                return header; // struct must be empty

            if (callOpCode != 0xE8)
                return header; // call &@100 not found. 
            
            // address is an offset
            // in IAT for @100 procedure. (name: ThunRTMain)
            
            // second thing: I don't know type of offset
            // after entryPoint. I suppose this is an absolute.
            _reader.BaseStream.Position += pushAddress;
            
            header = Fill<Vb5Header>(_reader);
        }
        catch
        {
            // ignored
        }

        return header;
    }
}