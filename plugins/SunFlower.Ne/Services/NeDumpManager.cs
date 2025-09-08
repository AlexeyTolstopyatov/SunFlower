using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.VisualBasic;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeDumpManager : UnsafeManager
{
    public MzHeader MzHeader { get; set; }
    public NeHeader NeHeader { get; set; }
    /// <summary>
    /// Processed EntryBundles of NE image
    /// </summary>
    public List<NeEntryBundle> EntryBundles { get; set; } = [];
    /// <summary>
    /// Importing names and procedures model
    /// </summary>
    public List<ImportModel> ImportModels { get; set; } = [];
    /// <summary>
    /// Exporting procedure names with implicit set ordinal in ".def" project file
    /// </summary>
    public List<Name> NonResidentNames { get; set; } = [];
    /// <summary>
    /// Module references table
    /// </summary>
    public List<ushort> ModuleReferences { get; set; } = [];
    /// <summary>
    /// Processed per-segment records collection
    /// </summary>
    public List<SegmentModel> Segments { get; set; } = [];
    /// <summary>
    /// Exporting procedures without implicit ordinal in ".def" file declared
    /// </summary>
    public List<Name> ResidentNames { get; set; } = [];
    
    /// <summary>
    /// Segmented EXE header offset
    /// </summary>
    private uint _offset = 0;

    /// <returns> Raw file address </returns>
    private uint Offset(uint address)
    {
        return _offset + address;
    }

    public NeDumpManager(string path)
    {
        Initialize(path);
    }

    private void Initialize(string path)
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        MzHeader = Fill<MzHeader>(reader);

        if (MzHeader.e_sign != 0x5a4d && MzHeader.e_sign != 0x4d5a) // cigam is very old sign but it also exists
            throw new InvalidOperationException("Doesn't have DOS/2 signature");

        _offset = MzHeader.e_lfanew;
        stream.Position = _offset;

        NeHeader = Fill<NeHeader>(reader);

        if (NeHeader.NE_ID != 0x454e && NeHeader.NE_ID != 0x4e45) // magic or cigam
            throw new InvalidOperationException("Doesn't have 'NE' signature");

        var exports = new NeNamesTablesManager(reader, new ExportOffsets(
            Offset(NeHeader.NE_ResidentNamesTable),
            NeHeader.NE_NonResidentNamesTable,
            NeHeader.NE_NonResidentNamesCount
        ));
        var imports = new NeImportNamesManager(reader, new ImportOffsets(
            NeHeader.NE_ImportModulesTable,
            1,
            Offset(NeHeader.NE_ModReferencesTable),
            NeHeader.NE_ModReferencesCount
        ));
        var segments = new NeSegmentTableManager(reader, 
            Offset(NeHeader.NE_SegmentsTable), 
            NeHeader.NE_SegmentsCount,
            NeHeader.NE_Alignment);
        var entries = new NeEntryTableManager(reader, 
            Offset(NeHeader.NE_EntryTable), 
            NeHeader.NE_EntriesCount);

        Segments = segments.SegmentModels;
        EntryBundles = entries.EntryBundles;
        ResidentNames = exports.ResidentNames;
        NonResidentNames = exports.NonResidentNames;
        ImportModels = imports.ImportModels;
        ModuleReferences = imports.ModuleReferences;
        
        reader.Close();
    }
    
}