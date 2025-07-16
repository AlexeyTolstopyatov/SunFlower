using System.Diagnostics;
using System.Text;
using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

///
/// CoffeeLake 2024-2025
/// This code is JellyBins part for dumping
/// Windows PE32/+ images.
///
/// Licensed under MIT
/// 

/// <summary>
/// Opens stream and makes dump for physical sections
/// in PE32/+ required image
/// </summary>
/// <param name="info"></param>
public class PeExportsManager(FileSectionsInfo info, string path) : DirectoryManager(info), IManager
{
    private readonly FileSectionsInfo _info = info;
    public PeExportTableModel ExportTableModel { get; private set; }

    public static PeExportsManager CreateInstance(FileSectionsInfo info, string path)
    { 
        return new(info, path);
    }
    
    // Declare Imports Exports CRT BaseRelocs (and other) sections here.
    public void Initialize()
    {
        // run main process
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        ExportTableModel = FillExportTableModel(reader);
        
        reader.Close();
    }

    private PeExportTableModel FillExportTableModel(BinaryReader reader)
    {
        // make sure: ExportsDirectory exists
        if (!IsDirectoryExists(_info.Directories[0]))
            return new();
        
        PeExportTableModel model = new();
        
        var exportRva = _info.Directories[0].VirtualAddress;
        var exportOffset = Offset(exportRva);

        reader.BaseStream.Seek(exportOffset, SeekOrigin.Begin);
        var exportDir = Fill<PeImageExportDirectory>(reader);
        model.ExportDirectory = exportDir; // <-- ExportDirectory added
        
        reader.BaseStream.Position = Offset(exportDir.Name);
        
        var moduleName = ReadImportString(reader);
        Debug.WriteLine(moduleName);
        
        var functionAddresses = ReadArray<UInt32>(reader, exportDir.AddressOfFunctions, exportDir.NumberOfFunctions);
        var namePointers = ReadArray<UInt32>(reader, exportDir.AddressOfNames, exportDir.NumberOfNames);
        var ordinals = ReadArray<UInt16>(reader, exportDir.AddressOfNameOrdinals, exportDir.NumberOfNames);

        for (var i = 0; i < exportDir.NumberOfNames; i++)
        {
            var functionName = ReadExportString(reader, namePointers[i]);
            var ordinal = (ordinals[i] + exportDir.Base);
            var address = _info.Is64Bit 
                ? ReadArray<UInt64>(reader, functionAddresses[ordinals[i]], 1)[0] 
                : ReadArray<UInt32>(reader, functionAddresses[ordinals[i]], 1)[0];

            model.Functions.Add(new ExportFunction // <-- Exported Functions added
            {
                Name = functionName,
                Ordinal = ordinal,
                Address = address
            });
        }
        
        return model;
    }
    
    /// <param name="reader"> <see cref="BinaryReader"/> instance </param>
    /// <returns> ASCIIZ typed string <c>TSTR</c> </returns>
    private static String ReadImportString(BinaryReader reader)
    {
        List<Byte> bytes = [];
        Byte b;
        while ((b = reader.ReadByte()) != 0)
            bytes.Add(b);
        
        return Encoding.ASCII.GetString(bytes.ToArray());
    }
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <param name="rva">rva of entry name</param>
    /// <returns> ASCII(Z) string of exported entry</returns>
    private String ReadExportString(BinaryReader reader, UInt32 rva)
    {
        var offset = Offset(rva);
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        List<Byte> bytes = [];
        Byte b;
        while ((b = reader.ReadByte()) != 0)
            bytes.Add(b);
        return Encoding.ASCII.GetString(bytes.ToArray());
    }
}