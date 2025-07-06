using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.VisualBasic;
using SunFlower.Models;

namespace SunFlower;
/// <summary>
/// Builtin service for recognition image by filename.
/// </summary>
public class ImageReader
{
    public static ImageReaderResult GetImageResults(string path)
    {
        ImageReaderResult result = new()
        {
            Path = path,
            Name = new FileInfo(path).Name,
            CpuArchitecture = "?",
            Signature = "?"
        };
        
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        UInt16 mzSignature = reader.ReadUInt16();

        if (mzSignature == 0x5a4d || mzSignature == 0x4d5a)
        {
            return GetMicrosoftImageResult(reader, ref result);
        }

        // ELF/A-OUT/MACH-obj/COFF/OMF supports follows next
        
        return result;
    }

    private static ImageReaderResult GetMicrosoftImageResult(BinaryReader reader, ref ImageReaderResult result)
    {
        // find next sign by pointer
        reader.BaseStream.Position = 0x3C;
        UInt32 dwNewHeaderPointer = reader.ReadUInt32();

        reader.BaseStream.Position = dwNewHeaderPointer;
        UInt32 dwNewHeader = reader.ReadUInt32();
        
        switch (dwNewHeader)
        {
            case 0x454e:
                result.Signature = "New Executable (NE16)";
                result.CpuArchitecture = "IA-32"; // Exists only for x86  
                break;
            case 0x454c:
                result.Signature = "Linear Executable (LE16/32)";
                break;
            case 0x584c: // supports alpha/ppc/x86
                result.Signature = "Linear Executable (LX32)";
                break;
            case 0x4550: // supports dohuya (see PE Format by microsoft.com)
                result.Signature = "Portable Executable (PE32/+)";
                break;
            default:     // DOS/2x bin, supports only x86
                result.Signature = $"DOS 2.x Executable (MZ16)";
                result.CpuArchitecture = "IA-32";
                break;
        }
        
        #if DEBUG // more information about offset
        reader.BaseStream.Position = dwNewHeaderPointer;
        char[] bNewHeaderAsciiString = reader.ReadChars(4);
        
        Debug.WriteLine($"{new string(bNewHeaderAsciiString)} (full: {dwNewHeader})");
        #endif
        
        // Need to find DOS/16 executables
        
        return result;
    }
}