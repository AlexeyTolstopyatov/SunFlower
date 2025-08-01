﻿using System.Runtime.InteropServices;

namespace SunFlower.Pe.Services;

///
/// CoffeeLake 2024-2025
/// This code is JellyBins part for dumping
/// Windows PE32/+ images.
///
/// Licensed under MIT
/// 

public class UnsafeManager
{
    /// <param name="reader"><see cref="BinaryReader"/> instance </param>
    /// <typeparam name="TStruct">structure</typeparam>
    /// <returns></returns>
    protected TStruct Fill<TStruct>(BinaryReader reader) where TStruct : struct
    {
        var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(TStruct)));
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        var result = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();
        
        return result;
    }
}