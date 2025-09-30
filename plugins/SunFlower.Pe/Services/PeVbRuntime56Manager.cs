using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeVbRuntime56Manager : DirectoryManager
{
    public Vb5Header Vb5Header { get; }
    public VbComData VbComData { get; }
    public VbDesignerInfo VbDesignerInfo { get; }
    
    private readonly FileSectionsInfo _info;
    private readonly BinaryReader _reader;
    private long _vbanew;

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
                return header; // callf @100 not found. 
            
            _reader.BaseStream.Position = (pushAddress - _info.ImageBase); // FINALLY!!!!!!! GOD THANK YOU!!!!!
            _vbanew = _reader.BaseStream.Position;
            
            header = Fill<Vb5Header>(_reader);

            if (new string(header.VbMagic) != "VB5!")
            {
                header = new Vb5Header();
            }
        }
        catch
        {
            // ignored
        }

        return header;
    }
}