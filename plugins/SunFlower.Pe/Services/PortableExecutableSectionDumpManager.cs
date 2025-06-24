using System.Runtime.InteropServices;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

/// <summary>
/// Opens stream and makes dump for physical sections
/// in PE32/+ required image
/// </summary>
/// <param name="info"></param>
public class PortableExecutableSectionDumpManager(FileSectionsInfo info, string path) : IManager
{
    // Declare Imports Exports CRT BaseRelocs (and other) sections here.
    
    public void Initialize()
    {
        Task.Run(async() =>
        {
            // run main process
            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new(stream);
            
            await Task.Delay(100);
        });
    }
    
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <typeparam name="TStruct">structure</typeparam>
    /// <returns></returns>
    private TStruct Fill<TStruct>(BinaryReader reader) where TStruct : struct
    {
        Byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(TStruct)));
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        TStruct result = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();
        
        return result;
    }
}