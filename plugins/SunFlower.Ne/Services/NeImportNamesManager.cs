using System.Numerics;
using System.Text;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public struct ImportOffsets(uint imptab, uint cbimp, uint modtab, uint cbmod)
{
    public uint ImportingModulesOffset { get; set; } = imptab;
    public uint ImportingModulesCount { get; set; } = cbimp;
    public uint ModuleReferencesOffset { get; set; } = modtab;
    public uint ModuleReferencesCount { get; set; } = cbmod;
}

public class NeImportNamesManager(BinaryReader reader, ImportOffsets offsets) : UnsafeManager
{
    public List<ushort> ModuleReferences { get; set; } = FillModuleReferences(reader, offsets);
    public List<ImportModel> ImportModels { get; set; } = FillImports(reader, offsets);
    
    /// <summary>
    /// Tries to fill suggesting imported module names and procedure names
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="offsets">structure of</param>
    private static List<ImportModel> FillImports(BinaryReader reader, ImportOffsets offsets)
    {
        List<ImportModel> imports = new();
        var importTableOffset = offsets.ImportingModulesOffset; // imports offset

        reader.BaseStream.Position = offsets.ModuleReferencesOffset;
        var moduleRefOffsets = new ushort[offsets.ModuleReferencesCount];
        for (var i = 0; i < moduleRefOffsets.Length; i++)
        {
            moduleRefOffsets[i] = reader.ReadUInt16();
        }

        foreach (var moduleNameOffset in moduleRefOffsets)
        {
            reader.BaseStream.Position = importTableOffset + moduleNameOffset;
            ImportModel moduleImport = new() { Functions = [] };
            
            // Module name check
            var nameLen = reader.ReadByte();
            moduleImport.DllName = Encoding.ASCII.GetString(reader.ReadBytes(nameLen));

            // Procedure name kek
            while (true)
            {
                var funcLen = reader.ReadByte();
                if (funcLen == 0) break;

                var isOrdinal = (funcLen & 0x80) != 0;
                var realLen = (byte)(funcLen & 0x7F);

                ImportingFunction func = new();

                if (isOrdinal) // <-- Module references are invalid. They took NonResidentNames table 
                {
                    var ordinal = reader.ReadUInt16();
                    func.Name = $"@{ordinal}";
                    func.Ordinal = ordinal;
                }
                else
                {
                    func.Name = Encoding.ASCII.GetString(reader.ReadBytes(realLen));
                    func.Ordinal = 0;
                }

                moduleImport.Functions.Add(func);
            }

            imports.Add(moduleImport);
        }

        // erase dll names from every Importing Function
        var dllNames = imports.Select(i => i.DllName).ToList();
        var erasedImportModels = new List<ImportModel>();
        
        foreach (var import in imports)
        {
            var notDllNames = import.Functions.Where(i => !dllNames.Contains(i.Name)).ToList();
            erasedImportModels.Add(new ImportModel
            {
                Functions = notDllNames,
                DllName = import.DllName
            });
        }
        return erasedImportModels;
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