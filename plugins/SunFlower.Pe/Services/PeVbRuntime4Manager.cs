using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeVbRuntime4Manager : DirectoryManager
{
    private readonly FileSectionsInfo _info;
    private readonly BinaryReader _reader;
    private long _vbanew;
    public Vb4Header Vb4Header { get; }

    public PeVbRuntime4Manager(FileSectionsInfo info, BinaryReader reader) : base(info)
    {
        _info = info;
        _reader = reader;

        Vb4Header = Find();
    }

    private Vb4Header Find()
    {
        var header = new Vb4Header();
        // recompute Image Base Alignment for a 1st time:
        // 
        // Dim ImageBaseAlignment As Long = ((OptHeader.ImageBase + OptHeader.EntryPoint) - GetPtrFromRVA(OptHeader.EntryPoint))
        // 
        var imageBaseAlignment = (_info.ImageBase + _info.EntryPoint) - Offset(_info.EntryPoint);
        try
        {
            var offset = Offset(_info.EntryPoint);
            _reader.BaseStream.Position = offset;

            var pushOpcode = _reader.ReadByte();    // push opcode
            var pushAddress = _reader.ReadUInt32(); // always 32-bit register.
            
            if (pushOpcode != 0x68) // <-- PUSH passed. CALL passed.
                return header;      // struct must be empty
                                    // CALL of @100 important for VB4 runtime init 

            _reader.BaseStream.Position = pushAddress - imageBaseAlignment; // not works.
            _vbanew = pushAddress - imageBaseAlignment; // <-- holds on the position of VB runtime header start.
            
            header = Fill<Vb4Header>(_reader);
        }
        catch
        {
            // ignoring
        }

        return header;
    }
}