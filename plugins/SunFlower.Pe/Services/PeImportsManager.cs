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
public class PeImportsManager(FileSectionsInfo info, string path) : DirectoryManager(info), IManager
{
    private readonly FileSectionsInfo _info = info;
    public PeImportTableModel ImportTableModel { get; private set; } = new();

    /// <summary> Deserializes bytes segment to import entries table </summary>
    /// <param name="reader">your content reader instance</param>
    /// <returns> Done <see cref="PeImportTableModel"/> structure </returns>
    private PeImportTableModel FillImportTableModel(BinaryReader reader)
    {
        PeImportTableModel dump = new();
        
        // make sure: Static Import entries exists
        if (!IsDirectoryExists(_info.Directories[1]))
            return new();
        
        try
        {
            reader.BaseStream.Seek(Offset(_info.Directories[1].VirtualAddress), SeekOrigin.Begin); // all sections instead IMPORTS
            List<PeImportDescriptor> items = [];
            while (true)
            {
                PeImportDescriptor item = Fill<PeImportDescriptor>(reader);
                if (item.OriginalFirstThunk == 0) break;

                items.Add(item);
            }

            foreach (PeImportDescriptor item in items)
            {
                reader.BaseStream.Seek(Offset(item.Name), SeekOrigin.Begin);
                Byte[] name = [];
                while (true)
                {
                    Byte b = Fill<Byte>(reader);
                    if (b == 0) break;

                    Byte[] dllName = new Byte[name.Length + 1];
                    name.CopyTo(dllName, 0);
                    dllName[name.Length] = b;
                    name = dllName;
                }

                Debug.WriteLine(Encoding.ASCII.GetString(name));
            }

            List<ImportModule> modules = [];
            foreach (PeImportDescriptor descriptor in items)
            {
                modules.Add(ReadImportDll(reader, descriptor));
            }

            dump.Modules = modules;
        }
        catch
        {
            // ignoring
        }
        
        return dump;
    }

    /// <summary> Deserializes bytes segment into IAT entries </summary>
    /// <param name="reader"></param>
    /// <returns> IAT model for current image </returns>
    private PeImportTableModel FillImportAddressesTableModel(BinaryReader reader)
    {
        UInt32 iatRva = _info.Directories[12].VirtualAddress;
        UInt32 iatSize = _info.Directories[12].Size;

        if (IsDirectoryExists(info.Directories[12])) 
            return new();

        Int64 iatOffset = Offset(iatRva);
        
        reader.BaseStream.Position = iatOffset;
        Int32 entrySize = _info.Is64Bit ? 8 : 4;
        List<UInt64> iatEntries = [];

        for (Int32 i = 0; i < iatSize / entrySize; i++)
        {
            UInt64 entry = _info.Is64Bit 
                ? reader.ReadUInt64() 
                : reader.ReadUInt32();
    
            if (entry == 0) 
                break;
            
            iatEntries.Add(entry);
        }

        // IAT records starts now
        foreach (UInt64 entry in iatEntries)
        {
            bool isOrdinal = (entry & (_info.Is64Bit 
                ? 0x8000000000000000 
                : 0x80000000)) != 0;
    
            // ... next logic ...
        }
    
        return new();
    }
    /// <param name="reader"> Current instance of <see cref="BinaryReader"/> </param>
    /// <param name="descriptor"> Seeking <see cref="PeImportDescriptor"/> table </param>
    /// <returns> Filled <see cref="ImportModule"/> instance full of module information </returns>
    private ImportModule ReadImportDll(BinaryReader reader, PeImportDescriptor descriptor)
    {
        Int64 nameOffset = Offset(descriptor.Name);
        reader.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
        String dllName = ReadImportString(reader);
        Debug.WriteLine($"IMAGE_IMPORT_TABLE->{dllName}");
        
        // optional [?]
        List<ImportedFunction> oft = 
            ReadThunk(reader, descriptor.OriginalFirstThunk, "[By OriginalFirstThunk]");

        List<ImportedFunction> ft = 
            ReadThunk(reader, descriptor.FirstThunk, "[By FirstThunk]");

        List<ImportedFunction> functions = [];
        functions.AddRange(oft);
        // functions.AddRange(ft); // <-- may contains duplicates of imported entries

        return new ImportModule { DllName = dllName, Functions = functions };
    }
    /// <summary> Use it when application requires 32bit machine WORD </summary>
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <param name="thunkRva">RVA of procedures block</param>
    /// <param name="tag">debug information (#debug only)</param>
    /// <returns>List of imported functions</returns>
    private List<ImportedFunction> ReadThunk(BinaryReader reader, UInt32 thunkRva, String tag)
    {
        int sizeOfThunk = _info.Is64Bit ? 8 : 4;
        UInt64 ordinalBit = _info.Is64Bit ? 0x8000000000000000 : 0x80000000;
        UInt64 ordinalMask = _info.Is64Bit ? 0x7FFFFFFFFFFFFFFFu : 0x7FFFFFFFu;
    
        List<ImportedFunction> result = [];
        if (thunkRva == 0) 
            return result;
    
        try
        {
            Int64 thunkOffset = Offset(thunkRva);
            while (true)
            {
                reader.BaseStream.Position = thunkOffset;
            
                UInt64 thunkValue = _info.Is64Bit 
                    ? reader.ReadUInt64() 
                    : reader.ReadUInt32();
            
                if (thunkValue == 0) 
                    break;
                
                // IMPORT by @ordinal
                if ((thunkValue & ordinalBit) != 0)
                {
                    result.Add(new ImportedFunction
                    {
                        Name = $"@{thunkValue & ordinalMask}",
                        Ordinal = (uint)(thunkValue & ordinalMask),
                        Hint = 0,
                        Address = thunkValue
                    });
                }
                // IMPORT by name
                else
                {
                    Int64 nameAddr = Offset((UInt32)thunkValue);
                    reader.BaseStream.Position = nameAddr;
                
                    UInt16 hint = reader.ReadUInt16();
                    string name = ReadImportString(reader);
                
                    result.Add(new ImportedFunction
                    {
                        Name = name,
                        Hint = hint,
                        Address = thunkValue
                    });
                }
            
                thunkOffset += sizeOfThunk;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{tag} error: {ex.Message}");
        }

        return result;
    }
    /// <param name="reader"> <see cref="BinaryReader"/> instance </param>
    /// <returns> ASCII terminated string <c>TSTR</c> </returns>
    /// <remarks>terminated means zeroed (has <c>\0</c> at the end) </remarks>
    private static String ReadImportString(BinaryReader reader)
    {
        List<Byte> bytes = [];
        Byte b;
        while ((b = reader.ReadByte()) != 0)
            bytes.Add(b);
        
        return Encoding.ASCII.GetString(bytes.ToArray());
    }
    /// <summary>
    /// Entry point of this manager
    /// </summary>
    public void Initialize()
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        ImportTableModel = FillImportTableModel(reader);
        
        reader.Close();
    }
}