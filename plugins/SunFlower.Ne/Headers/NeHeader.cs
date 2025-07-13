using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

public struct NeHeader
{ 
    public NeHeader() {}
    
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ID = 0; 
    [MarshalAs(UnmanagedType.U1)] public byte NE_LinkerVersion = 0; 
    [MarshalAs(UnmanagedType.U1)] public byte NE_LinkerRevision = 0; 
    [MarshalAs(UnmanagedType.U2)] public ushort NE_EntryTable = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_EntriesCount = 0;
    [MarshalAs(UnmanagedType.U4)] public uint NE_Checksum = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_ProgramFlags = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_AppFlags = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_AutoSegment = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Heap = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Stack = 0;
    [MarshalAs(UnmanagedType.U4)] public uint NE_CsIp = 0;
    [MarshalAs(UnmanagedType.U4)] public uint NE_SsSp = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SegmentsCount = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ModReferencesCount = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_NonResidentNamesCount = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SegmentsTable = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResourcesTable = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResidentNamesTable = 0; 
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ModReferencesTable = 0; 
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ImportModulesTable = 0;
    [MarshalAs(UnmanagedType.U4)] public uint NE_NonResidentNamesTable = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort cmovent = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_Alignment = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_ResourcesCount = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_OS = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_FlagOthers = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort pretthunks = 0; 
    [MarshalAs(UnmanagedType.U2)] public ushort psegrefbytes = 0;
    [MarshalAs(UnmanagedType.U2)] public ushort NE_SwapArea = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_WindowsVersionMinor = 0;
    [MarshalAs(UnmanagedType.U1)] public byte NE_WindowsVersionMajor = 0;
}