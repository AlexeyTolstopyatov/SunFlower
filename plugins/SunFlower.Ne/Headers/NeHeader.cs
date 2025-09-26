using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeHeader
{
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ID;
    [MarshalAs(UnmanagedType.U1)] public byte NE_LinkerVersion;
    [MarshalAs(UnmanagedType.U1)] public byte NE_LinkerRevision;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_EntryTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_EntriesCount;
    [MarshalAs(UnmanagedType.U4)] public uint NE_Checksum;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Flags;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_AutoSegment;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Heap;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Stack;
    [MarshalAs(UnmanagedType.U4)] public uint NE_CsIp;
    [MarshalAs(UnmanagedType.U4)] public uint NE_SsSp;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SegmentsCount;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ModReferencesCount;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_NonResidentNamesCount;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SegmentsTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResourcesTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResidentNamesTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ModReferencesTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ImportModulesTable;
    [MarshalAs(UnmanagedType.U4)] public uint NE_NonResidentNamesTable;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_MovableEntriesCount;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Alignment; // log() | 0 eq 9
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResourcesCount;
    [MarshalAs(UnmanagedType.U1)] public byte NE_OS;
    [MarshalAs(UnmanagedType.U1)] public byte NE_FlagOthers;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_PretThunks;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_PerSegmentRefByte;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SwapArea;
    [MarshalAs(UnmanagedType.U1)] public byte NE_WindowsVersionMinor;
    [MarshalAs(UnmanagedType.U1)] public byte NE_WindowsVersionMajor;
}