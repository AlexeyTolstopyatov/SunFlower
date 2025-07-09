using SunFlower.Le.Headers.Le;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Services;
using System.Text;

namespace SunFlower.Le.Services;

public class LeDumpManager : UnsafeManager
{
    private UInt32 _offset;

    public LeDumpManager(string path)
    {
        Initialize(path);
    }

    public MzHeader MzHeader { get; set; } = new();
    public LeHeader LeHeader { get; set; } = new();
    public VddHeader DriverHeader { get; set; }
    public List<ResidentName> ResidentNames { get; set; } = [];
    public List<NonResidentName> NonResidentNames { get; set; } = [];
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];
    public List<EntryBundle> EntryBundles { get; set; } = [];
    public UInt32 Offset(UInt32 address) => _offset + address;

    private void Initialize(string path)
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        MzHeader = Fill<MzHeader>(reader);

        if (MzHeader.e_sign != 0x5a4d && MzHeader.e_sign != 0x4d5a) // cigam is very old sign but it also exists
            throw new InvalidOperationException("Doesn't have DOS/2 signature");

        _offset = MzHeader.e_lfanew;
        stream.Position = _offset;

        LeHeader = Fill<LeHeader>(reader);

        if (LeHeader.LE_ID != 0x454c && LeHeader.LE_ID != 0x4c45) // magic or cigam
            throw new InvalidOperationException("Doesn't have new signature");

        FillNames(reader);
        FillImportingNames(reader);
        FillEntryPoints(reader);
    }

    private void FillNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ResidentNames);
        byte size = reader.ReadByte();
        while (size != 0)
        {
            ResidentName name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Ordinal = reader.ReadUInt16(),
                Size = size
            };
            ResidentNames.Add(name);
            size = reader.ReadByte();
        }

        reader.BaseStream.Position = LeHeader.LE_NoneRes;
        size = reader.ReadByte();

        while (size != 0)
        {
            NonResidentName name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Ordinal = reader.ReadUInt16(),
                Size = size
            };
            NonResidentNames.Add(name);
            size = reader.ReadByte();
        }
    }

    private void FillImportingNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ImportModNames);

        byte size = reader.ReadByte();
        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingModules.Add(name);
            size = reader.ReadByte();
        }

        reader.BaseStream.Position = Offset(LeHeader.LE_ImportNames);
        size = reader.ReadByte();

        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingProcedures.Add(name);
            size = reader.ReadByte();
        }
    }
    
    /// <summary>
    /// If EntryPoints exists, some of them stores in <see cref="ResidentNames"/>
    /// , some of them in <see cref="NonResidentNames"/>, and their Ordinals in
    /// Non/Resident tables are pointers to EntryTable. (they tell "Where in EntryTable entry stores")
    /// </summary>
    /// <param name="reader"></param>
    private void FillEntryPoints(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_EntryTable);

        byte bundleSize;
        while ((bundleSize = reader.ReadByte()) != 0)
        {
            byte bundleFlags = reader.ReadByte(); 
            ushort objectIndex = reader.ReadUInt16();
            
            EntryBundle bundle = new()
            {
                EntriesCount = bundleSize,
                EntryBundleIndex = bundleFlags,
                ObjectIndex = objectIndex,
            };

            bool is32bit = (bundleFlags & 0b00000010) != 0;

            List<Entry16> entries = [];
            List<Entry32> entry32s = [];
            for (int i = 0; i < bundleSize; i++)
            {
                byte entryFlags = reader.ReadByte();

                if (is32bit)
                {
                    uint offset = reader.ReadUInt32();
                    entry32s.Add(new Entry32
                    {
                        Flag = entryFlags,
                        Offset = offset
                    });
                }
                else
                {
                    ushort offset = reader.ReadUInt16();
                    entries.Add(new Entry16
                    {
                        Flag = entryFlags,
                        Offset = offset
                    });
                }
            }

            bundle.Entries = entries.ToArray();
            bundle.ExtendedEntries = entry32s.ToArray();

            EntryBundles.Add(bundle);
        }
    }

    private void FillObjectTable()
    {
        // There;s no sections yet.
        // Here uses something little same about file segment and logical partition
        // of file. It's called Object in OS2_OMF_Format_And_Linear_Executable.pdf and eComStation_LE_Manual.pdf
    }
}