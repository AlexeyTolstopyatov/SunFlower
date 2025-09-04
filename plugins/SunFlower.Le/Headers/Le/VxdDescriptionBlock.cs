using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VxdDescriptionBlock
{
    public uint Next;         /* VMM RESERVED FIELD */
    public ushort Version;     /* INIT <DDK_VERSION> RESERVED FIELD */
    public ushort RequiredDeviceNumber;   /* INIT <UNDEFINED_DEVICE_ID> */
    public byte DevMajor;    /* INIT <0> Major device number */
    public byte DevMinor;    /* INIT <0> Minor device number */
    public ushort Flags;           /* INIT <0> for init calls complete */
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Name;          /* 8 bytes AINIT <"        "> Device name */
    
    public uint InitOrder;       /* INIT <UNDEFINED_INIT_ORDER> */
    public uint ControlProcedureOffset;     /* Offset of control procedure */
    public uint V86ApiProcedureOffset;     /* INIT <0> Offset of API procedure */
    public uint PmApiProcedure;      /* INIT <0> Offset of API procedure */
    public uint V86CsIp;     /* INIT <0> CS:IP of API entry point */
    public uint PmApiCsIp;      /* INIT <0> CS:IP of API entry point */
    public uint RealModeReferenceData;       /* Reference data from real mode */
    public uint ServiceTablePointer;    /* INIT <0> Pointer to service table */
    public uint ServiceTableSize;   /* INIT <0> Number of services */
    public uint Win32ServiceTable;  /* INIT <0> Pointer to Win32 services */
    public uint Prev4;         /* INIT <'Prev'> Ptr to prev 4.0 DDB */
    public uint Reserved0;        /* INIT <0> Reserved */
    public uint Reserved1;        /* INIT <'Rsv1'> Reserved */
    public uint Reserved2;        /* INIT <'Rsv2'> Reserved */
    public uint Reserved3;        /* INIT <'Rsv3'> Reserved */
}