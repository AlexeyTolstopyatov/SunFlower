using System.Diagnostics;
using System.Text;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public struct ImportOffsets(uint imptab, uint cbimp, uint modtab, uint cbmod)
{
    public uint ImportingModulesOffset { get; set; } = imptab;
    public uint ImportingModulesCount { get; set; } = cbimp;
    public uint ModuleReferencesOffset { get; } = modtab;
    public uint ModuleReferencesCount { get; } = cbmod;
}

public class NeImportNamesManager(BinaryReader reader, ImportOffsets offsets, List<SegmentModel> segmentModels) : UnsafeManager
{
    public List<ushort> ModuleReferences { get; } = FillModuleReferences(reader, offsets);
    public Dictionary<string, List<Import>> ImportModels { get; } = FillImports(reader, offsets, segmentModels);
    
    /// <summary>
    /// Tries to fill suggesting imported module names and procedure names
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="offsets">structure of</param>
    private static Dictionary<string, List<Import>> FillImports(BinaryReader reader, ImportOffsets offsets, List<SegmentModel> segmentModels)
    {
        // fill ModuleReferences Table
        reader.BaseStream.Position = offsets.ModuleReferencesOffset;
        var modTab = new List<ushort>();
        
        for (int i = 0; i < offsets.ModuleReferencesCount; ++i)
        {
            modTab.Add(reader.ReadUInt16());
        }
        
        var importedModules = new Dictionary<string, List<Import>>();
        
        // exclude all non-import records
        var importRelocations = segmentModels
            .SelectMany(segment => segment.Relocations)
            .Where(reloc => reloc.RelocationType.Equals("Import", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var relocation in importRelocations)
        {
            string moduleName = GetModule(reader, offsets, relocation.ModuleIndex);
            
            if (string.IsNullOrEmpty(moduleName))
                continue;

            Import procedure = GetImport(reader, offsets, relocation);
            procedure.Module = moduleName;
            
            if (!importedModules.ContainsKey(moduleName))
            {
                importedModules[moduleName] = new List<Import>();
            }
            
            if (!importedModules[moduleName].Any(p => 
                    (p.Ordinal != "@0" && p.Ordinal == procedure.Ordinal) ||
                    (!string.IsNullOrEmpty(p.Procedure) && p.Procedure == procedure.Procedure)))
            {
                importedModules[moduleName].Add(procedure);
            }
        }

        foreach (var module in importedModules)
        {
            module.Value.Sort((a, b) => 
            {
                if (a.Ordinal != "@0" && b.Ordinal != "@0")
                    return String.Compare(a.Ordinal, b.Ordinal, StringComparison.Ordinal);
                if (a.Ordinal != "@0") return -1;
                if (b.Ordinal != "@0") return 1;
                return string.Compare(a.Procedure, b.Procedure, StringComparison.Ordinal);
            });
        }

        return importedModules;
    }
    private static string GetModule(BinaryReader reader, ImportOffsets offsets, ushort moduleIndex)
    {
        try
        {
            if (moduleIndex == 0) return string.Empty;
            
            reader.BaseStream.Position = offsets.ModuleReferencesOffset + 2 * (moduleIndex - 1);
            var moduleNameOffset = reader.ReadUInt16();
            
            reader.BaseStream.Position = offsets.ImportingModulesOffset + moduleNameOffset;
            return ReadPascalString(reader);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading module name for index {moduleIndex}: {ex.Message}");
            return string.Empty;
        }
    }
    private static Import GetImport(BinaryReader reader, ImportOffsets offsets, Relocation relocation)
    {
        var procedure = new Import();
        procedure.NameOffset = relocation.NameOffset;
        procedure.ModuleIndex = relocation.ModuleIndex;
        procedure.OffsetInSegment = relocation.OffsetInSegment;
        
        if (relocation.Ordinal > 0)
        {
            procedure.Ordinal = "@" + relocation.Ordinal;
            procedure.Procedure = $"@{relocation.Ordinal}";
        }
        else if (relocation.NameOffset > 0)
        {
            try
            {
                reader.BaseStream.Position = offsets.ImportingModulesOffset + relocation.NameOffset;
                procedure.Procedure = ReadPascalString(reader);
                procedure.Ordinal = "@0";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading procedure name at offset {relocation.NameOffset}: {ex.Message}");
                procedure.Procedure = $"?0x{relocation.NameOffset:X8}";
            }
        }
        else
        {
            procedure.Procedure = "Anonymous";
        }
        
        return procedure;
    }
    private static string ReadPascalString(BinaryReader reader)
    {
        // position already set;
        var pstrLength = reader.ReadByte();

        return pstrLength == 0 ? "" : Encoding.ASCII.GetString(reader.ReadBytes(pstrLength));
    }
    
    private static List<ushort> FillModuleReferences(BinaryReader reader, ImportOffsets offsets)
    {
        reader.BaseStream.Position = offsets.ModuleReferencesOffset;
        var modules = new List<ushort>();

        for (var i = 0; i < offsets.ModuleReferencesCount; i++)
        {
            var mod = reader.ReadUInt16();
            modules.Add(mod);
        }

        return modules;
    }
}