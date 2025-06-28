using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct MzHeader
{
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_sign;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_lastb;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_fbl;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_relc;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_pars;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_minep;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_maxep;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ss;
    [MarshalAs(UnmanagedType.U2)] public UInt16 sp;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_check;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ip;
    [MarshalAs(UnmanagedType.U2)] public UInt16 cs;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_reltableoff;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_overnum;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public UInt16[] e_res0x1c;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_oemid;
    [MarshalAs(UnmanagedType.U2)] public UInt16 e_oeminfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public UInt16[] e_res_0x28;
    [MarshalAs(UnmanagedType.U4)] public UInt32 e_lfanew;
}