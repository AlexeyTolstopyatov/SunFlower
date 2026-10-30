//
// CoffeeLake (C) 
//
namespace SunFlower.Abstractions

open System.IO
open System.Runtime.InteropServices

type BinaryReaderExtension = 
    /// <summary>
    /// Reads custom "by-value" type using ready binary reader instance.
    /// Same with all <c>BinaryReader::Read...()</c> methods, moves reader and stream position 
    /// </summary>
    /// <param name="reader">
    /// Correctly initialized BinaryReader instance
    /// </param>
    /// <exception cref="EndOfStreamException">Bytes location are out of memory/file bounds</exception>
    /// <exception cref="IOException">I/O Exception</exception>
    /// <exception cref="ArgumentException">
    /// The number of decoded characters to read is greater than count.
    /// This can happen if a Unicode decoder returns fallback characters or a surrogate pair
    /// </exception>
    /// <exception cref="ObjectDisposedException">Stream instance is closed</exception>
    member public _.ReadType<'TManaged when 'TManaged : struct>(reader: BinaryReader) = 
        let size = Marshal.SizeOf<'TManaged>()
        let bytes = reader.ReadBytes(size)
        let typeHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
        let result = Marshal.PtrToStructure<'TManaged>(typeHandle.AddrOfPinnedObject())
        
        typeHandle.Free()
        
        result