namespace SunFlower.Le.Headers.Le;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LeHeader
{
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_ID;                // "LE" text identifier.
    
    [MarshalAs(UnmanagedType.U1)]
    public byte LE_ByteOrder;          // byte order, 0=little-endian, none-zero=big.
    
    [MarshalAs(UnmanagedType.U1)]
    public byte LE_WordOrder;          // word order.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Format;            // format level.
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_CPU;               // CPU type.
    
    public const ushort LeCpu286 = 1;
    public const ushort LeCpu386 = 2;
    public const ushort LeCpu486 = 3;
    public const ushort LeCpu586 = 4;
    public const ushort LeCpuI860 = 0x20;
    public const ushort LeCpuN11 = 0x21;
    public const ushort LeCpuR2000 = 0x40;
    public const ushort LeCpuR6000 = 0x41;
    public const ushort LeCpuR4000 = 0x42;
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_OS;                // Target operating system.
    
    public const ushort LeOsOs2 = 1;
    public const ushort LeOsWindows = 2;
    public const ushort LeOsDos4 = 3;
    public const ushort LeOsWin386 = 4;
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_VersionMajor;           // Module version.
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_VersionMinor;
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Type;              // Module type.
    
    public const uint LeTypeInitPer = 1 << 2;     // initialise per process.
    public const uint LeTypeIntFixup = 1 << 4;     // no internal fixups.
    public const uint LeTypeExtFixup = 1 << 5;     // no external fixups.
    public const uint LeTypeNoLoad = 1 << 13;    // module not loadable.
    public const uint LeTypeDll = 1 << 15;    // DLL
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Pages;             // number of memory pages.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_EntryCS;           // Entry CS object.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_EntryEIP;          // Entry EIP.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_EntrySS;           // Entry SS object.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_EntryESP;          // Entry ESP.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_PageSize;          // Page size.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_LastBytes;         // Bytes on last page.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_FixupSize;         // fixup section size.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_FixupChk;          // fixup section check sum.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_LoaderSize;        // loader section size.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_LoaderChk;         // loader section check sum.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ObjOffset;         // offset of object table.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ObjNum;            // object table entries
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_PageMap;           // object page map table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_IterateMap;        // object iterate data map offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Resource;          // resource table offset
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ResourceNum;       // resource table entries.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ResidentNames;     // resident names table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_EntryTable;        // entry table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Directives;        // module directives table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_DirectivesNum;     // module directives entries.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Fixups;            // fixup page table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_FixupsRec;         // fixup record table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ImportModNames;    // imported module name table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ImportModNum;      // imported modules count.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_ImportNames;       // imported procedures name table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_PageChk;           // per-page checksum table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Data;              // data pages offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_PreLoadNum;        // pre-load page count.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_NoneRes;           // non-resident names table offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_NoneResSize;       // non-resident names table length.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_NoneResChk;        // non-resident names checksum.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_AutoDS;            // automatic data object.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_Debug;             // debug information offset.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_DebugSize;         // debug information size.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_PreLoadInstNum;    // pre-load instance pages number.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_DemandInstNum;     // demand instance pages number.
    
    [MarshalAs(UnmanagedType.U4)]
    public uint LE_HeapExtra;         // extra heap allocation.
    
    // <=> pages shift +4h
    // <=> stackSize +4h
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] LE_Reserved;
}

