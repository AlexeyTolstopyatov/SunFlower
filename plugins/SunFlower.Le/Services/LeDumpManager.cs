using SunFlower.Le.Headers.Le;
using SunFlower.Le.Headers;
using SunFlower.Le.Models.Le;
using Object = SunFlower.Le.Headers.Le.Object;

namespace SunFlower.Le.Services;

public class LeDumpManager : UnsafeManager
{
    private UInt32 _offset;

    public LeDumpManager(string path)
    {
        _isVirtualDriver = false;
        Initialize(path);
    }

    public MzHeader MzHeader { get; set; }
    public LeHeader LeHeader { get; set; }
    public VxdHeader DriverHeader { get; set; }
    public List<Name> ResidentNames { get; set; } = [];
    public List<Name> NonResidentNames { get; set; } = [];
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];
    public List<EntryBundle> EntryBundles { get; set; } = [];
    public List<Object> Objects { get; set; } = [];
    public List<ObjectPageModel> ObjectPages { get; set; } = [];
    public List<uint> FixupPagesOffsets { get; set; } = [];
    public List<FixupRecordsTableModel> FixupRecords { get; set; } = [];
    public VxdDescriptionBlock DescriptionBlock { get; set; } = new();
    public VxdResources DriverResources { get; set; }
    public Win32Resource VersionInfo { get; set; }
    private UInt32 Offset(UInt32 address) => _offset + address;
    private bool _isVirtualDriver;
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
        var postHeadPosition = stream.Position;
        
        if (LeHeader.LE_ID != 0x454c && LeHeader.LE_ID != 0x4c45)
            if (LeHeader.LE_ID != 0x584c && LeHeader.LE_ID != 0x4c58)
                throw new NotSupportedException("Doesn't have 'LX' magic");

        var namesTables = new LeNamesTablesManager(reader, Offset(LeHeader.LE_ResidentNames), LeHeader.LE_NoneRes);
        var importNames = new LeImportNamesManager(reader, Offset(LeHeader.LE_ImportModNames), Offset(LeHeader.LE_ImportNames));
        var entryTable = new LeEntryTableManager(reader, Offset(LeHeader.LE_EntryTable));
        var objectTable = new LeObjectsManager(reader, Offset(LeHeader.LE_ObjOffset), LeHeader.LE_ObjNum);
        var pagesTable = new LePagesManager(reader, Offset(LeHeader.LE_PageMap), LeHeader.LE_Pages);

        NonResidentNames = namesTables.NonResidentNames;
        ResidentNames = namesTables.ResidentNames;
        ImportingModules = importNames.ImportingModules;
        ImportingProcedures = importNames.ImportingProcedures;
        EntryBundles = entryTable.EntryBundles;
        Objects = objectTable.Objects;
        ObjectPages = pagesTable.Pages;
        
        // anyway initialization
        var driver = new VxdDriverManager(reader, (uint)postHeadPosition);
        
        if (driver.DriverHeader.LE_WindowsResOffset == 0)
            return;
        
        if (LeHeader.LE_ID != 0x584c && LeHeader.LE_ID != 0x4c58)
            return;
        
        DriverHeader = driver.DriverHeader;
        DriverResources = driver.DriverResources;
        // DDB = driver.DDB;
        // Win32Resource = driver.Win32Resource;
        
        reader.Close();
    }
}