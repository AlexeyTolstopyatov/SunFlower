using SunFlower.Pe.Exceptions;
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
/// Directory manager must init following toolchain
/// </summary>
public class DirectoryManager(FileSectionsInfo info) : UnsafeManager
{
    /// <returns> Directory exists when someone of 2 parameters not 0 </returns>
    protected bool IsDirectoryExists(PeDirectory dir)
    {
        return dir.Size != 0 || dir.VirtualAddress != 0;
    }
    /// <param name="rva"> Required RVA </param>
    /// <returns> File offset from RVA of selected section </returns>
    /// <exception cref="SectionNotFoundException"> If RVA not belongs to any section </exception>
    protected Int64 Offset(Int64 rva)
    {
        PeSection section = Section(rva);
        
        return 0 + section.PointerToRawData + (rva - section.VirtualAddress);
    }
    /// <param name="rva"> Required relative address </param>
    /// <returns> <see cref="PeSection"/> Which RVA belongs </returns>
    /// <exception cref="SectionNotFoundException"> If RVA not belongs to any section </exception>
    protected PeSection Section(Int64 rva)
    {   // rva = {uint} 2019914798 
        // rva = {long} 2019914798 
        // RVA always 32-bit
        UInt32 rva32 = Convert.ToUInt32(rva); // instead casting
        foreach (PeSection section in info.Sections.OrderBy(s => s.VirtualAddress))
        {
            if (rva32 >= section.VirtualAddress && 
                rva32 < section.VirtualAddress + section.VirtualSize)
            {
                return section;
            }
        }
        throw new SectionNotFoundException();
    }
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <param name="rva">RVA</param>
    /// <param name="count">count of elements in segment</param>
    /// <typeparam name="T">type of array-segment</typeparam>
    /// <returns>Array of structures</returns>
    protected T[] ReadArray<T>(BinaryReader reader, UInt32 rva, UInt32 count) where T : struct
    {
        Int64 offset = Offset(rva);
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        T[] result = new T[count];
        for (Int32 i = 0; i < count; i++)
            result[i] = Fill<T>(reader);
        return result;
    }
}