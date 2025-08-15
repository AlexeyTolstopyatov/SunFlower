using SunFlower.Models;

namespace SunFlower;
/// <summary>
/// Builtin service for recognition image by filename.
///
/// Influences only on registry JSON list (see "recent.json"),
/// which YOU CAN ALWAYS edit manually
/// </summary>
public static class ImageReader
{
    /// <summary>
    /// Generic function, returns Image result anyway for every
    /// supported Binary File format by core.
    ///
    /// Core plugins /extensions in Project's solution/
    /// supports: MZ-e /extended/, PIF, PE32/+, LE16/+ NE/16 binaries.
    /// </summary>
    /// <param name="path">path to the required File</param>
    /// <returns>Filled <see cref="ImageReaderResult"/> structure </returns>
    public static ImageReaderResult Get(string path)
    {
        ImageReaderResult result = new()
        {
            Path = path,
            Name = new FileInfo(path).Name,
            CpuArchitecture = "?",
            SignatureString = "?",
        };
        
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        var mzSignature = reader.ReadUInt16();

        if (mzSignature is 0x5a4d or 0x4d5a)
        {
            return GetMicrosoftImageResult(reader, ref result);
        }

        // ELF/A-OUT/MACH-obj/COFF/OMF supports follows next
        
        return result;
    }
    /// <summary>
    /// Sees old Microsoft segmentation Formats. (PE32/+, LE16/+, NE16, MZ16)
    /// ONLY extended xx-DOS Mark Zbikowski (MZ) header. BW-DOS and other xx-DOS
    /// which has e_overlay field and relocations set in Standard MZ this function ignores.
    /// </summary>
    /// <param name="reader">current <see cref="BinaryReader"/> instance</param>
    /// <param name="result">current structure by pointer</param>
    private static ImageReaderResult GetMicrosoftImageResult(BinaryReader reader, ref ImageReaderResult result)
    {
        // find next sign by pointer
        reader.BaseStream.Position = 0x3C;
        var dwNewHeaderPointer = reader.ReadUInt32();

        reader.BaseStream.Position = dwNewHeaderPointer;
        var dwNewHeader = reader.ReadUInt32();
        result.SignatureDWord = dwNewHeader;
        
        switch (dwNewHeader)
        {
            case 0x454e or 0x4e45: // only IA-32. cigam
                result.SignatureString = "New Executable (NE16)";
                result.CpuArchitecture = "i286, IA-32";
                break;
            case 0x454c or 0x4c45: // magic \/ cigam
                result.SignatureString = "Linear Executable (LE16/+)";
                break;
            case 0x584c or 0x4c58: // supports alpha/ppc/IA-32
                result.SignatureString = "Linear Executable (LX32)";
                break;
            case 0x4550 or 0x5045: // don't actually know about cigam
                result.SignatureString = "Portable Executable (PE32/+)";
                break;
            default:     // DOS/2x bin, supports only x86
                result.SignatureString = $"DOS 2.x Executable (MZ16)";
                result.CpuArchitecture = "i8086+";
                break;
        }
        // Need to find BW-DOS DR-DOS executables
        
        return result;
    }
}