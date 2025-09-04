using SunFlower.Le.Headers;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Headers.Lx;
using SunFlower.Le.Models.Le;
using EntryBundle = SunFlower.Le.Headers.Lx.EntryBundle;
using Object = SunFlower.Le.Headers.Le.Object;

namespace SunFlower.Le.Services;

public class LxDumpManager : UnsafeManager
{
    public MzHeader MzHeader { get; set; }
    public LxHeader LxHeader { get; set; }
    public List<Name> ResidentNames { get; set; }
    public List<Name> NonResidentNames { get; set; }
    public List<Function> ImportingModules { get; set; }
    public List<Function> ImportingProcedures { get; set; }
    public List<EntryBundle> EntryBundles { get; set; }
    public List<Headers.Lx.Object> Objects { get; set; }
    public List<ObjectPageModel> Pages { get; set; }
    private uint _offset;

    public uint Offset(uint addr) => _offset + addr;
    
    public LxDumpManager(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        MzHeader = Fill<MzHeader>(reader);
        if (MzHeader.e_sign is not 0x5a4d and 0x4d5a)
            throw new NotSupportedException("Doesn't have MZ magic");

        _offset = MzHeader.e_lfanew;
        stream.Position = MzHeader.e_lfanew;
        LxHeader = Fill<LxHeader>(reader);
        if (LxHeader.e32_magic != 0x454c && LxHeader.e32_magic != 0x4c45)
            if (LxHeader.e32_magic != 0x584c && LxHeader.e32_magic != 0x4c58)
                throw new NotSupportedException("Doesn't have 'LX' magic");
        
        var namesTables = new LeNamesTablesManager(reader, Offset(LxHeader.e32_restab), LxHeader.e32_nrestab);
        var importNames = new LeImportNamesManager(reader, Offset(LxHeader.e32_impmod), Offset(LxHeader.e32_impproc));
        var entryTable = new LxEntryTableManager(reader, Offset(LxHeader.e32_enttab));
        var objectTable = new LxObjectsManager(reader, Offset(LxHeader.e32_objtab), LxHeader.e32_objcnt);
        var pagesTable = new LePagesManager(reader, Offset(LxHeader.e32_objmap), LxHeader.e32_pagesum);

        NonResidentNames = namesTables.NonResidentNames;
        ResidentNames = namesTables.ResidentNames;
        ImportingModules = importNames.ImportingModules;
        ImportingProcedures = importNames.ImportingProcedures;
        EntryBundles = entryTable.EntryBundles;
        Objects = objectTable.Objects;
        Pages = pagesTable.Pages;
        
        reader.Close();
    }
}