namespace SunFlower.Kernel.Database

open System.Runtime.InteropServices

/// Main structure of the sunflower object
[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
[<Struct>]
type FlowerObjectHeader =
    { /// ASCII string (8 characters)
      /// FLOWERED
      [<CompiledName "Magic"; MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)>]
      magic: char[]
      /// The version field equals the major version of the assembly which
      /// been created the binary
      [<CompiledName "Version">]
      version: uint32
      /// Revision field equals the minor version of the assembly
      /// which been created the binary
      [<CompiledName "Revision">]
      revision: uint32
      /// Integer Offset to the 01.01.2026:00:00:00 datetime
      /// Time sets in UTC
      [<CompiledName "CreatedTimeStamp">]
      createdStamp: uint64
      /// Integer offset to the 01.01.2026 datetime
      /// Time sets in UTC
      [<CompiledName "ModifiedTimeStamp">]
      modifiedStamp: uint64
      /// Size of the archive embedded in the binary
      /// By default the compression method will be PK (zip)
      /// but using extensions of a program - containers might be different
      [<CompiledName "ContainerSize">]
      containerSize: uint64 }