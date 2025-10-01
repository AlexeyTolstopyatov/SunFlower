using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

/* This information took from Semi VB Decompiler
   by VBGamer45
   
Private Type VB4HEADERType
    sig As Long 'SIG 129 53 84 182
    CompilerFileVersion As Integer
    int1 As Integer
    int2 As Integer
    int3 As Integer
    int4 As Integer
    int5 As Integer
    int6 As Integer
    int7 As Integer
    int8 As Integer
    int9 As Integer
    int10 As Integer
    int11 As Integer
    int12 As Integer
    int13 As Integer
    int14 As Integer
    int15 As Integer
    LangID As Integer
    int16 As Integer
    int17 As Integer
    int18 As Integer
    aSubMain As Long
    Address2 As Long
    i1 As Integer
    i2 As Integer
    i3 As Integer
    i4 As Integer
    i5 As Integer
    i6 As Integer
    iExeNameLength As Integer
    iProjectSavedNameLength As Integer
    iHelpFileLength As Integer
    iProjectNameLength As Integer
    FormCount As Integer
    int19 As Integer
    NumberOfExternalComponets As Integer
    int20 As Integer  'The same in each file 176d
    aGuiTable As Long  'GUI Pointer
    Address4 As Long
    aExternalComponetTable As Long '??Not a 100% sure
    aProjectInfo2 As Long  'Project Info2?  
End Type
 */

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vb4Header
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public Char[] Signature;
    [MarshalAs(UnmanagedType.U2)] public UInt16 CompilerVersion;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined1;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined2;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined3;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined4;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined5;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined6;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined7;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined8;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined9;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined10;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined11;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined12;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined13;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined14;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined15;
    [MarshalAs(UnmanagedType.U2)] public UInt16 LanguageDllId;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined16;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined17;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined18;
    [MarshalAs(UnmanagedType.U4)] public UInt32 SubMainAddress;
    [MarshalAs(UnmanagedType.U4)] public UInt32 Address;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined21;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined22;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined23;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined24;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined25;
    [MarshalAs(UnmanagedType.U2)] public UInt16 Undefined26;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ExeNameLength;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ProjectNameLength;
    [MarshalAs(UnmanagedType.U2)] public UInt16 FormsCount;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ModulesClassesCount;
    [MarshalAs(UnmanagedType.U2)] public UInt16 ExternComponentsCount;
    [MarshalAs(UnmanagedType.U2)] public UInt16 InEachFile176d;
    [MarshalAs(UnmanagedType.U4)] public UInt32 GuiTableOffset;
    [MarshalAs(UnmanagedType.U4)] public UInt32 UndefinedTableOffset;
    [MarshalAs(UnmanagedType.U4)] public UInt32 ExternComponentTableOffset; // not 100% sure
    [MarshalAs(UnmanagedType.U4)] public UInt32 ProjectInfoTableOffset;
}
[StructLayout(LayoutKind.Sequential)]
public struct OldVb4Header
{
    public Byte PushCode;
    public UInt32 PushAddress;
    public Byte CallProcedureCode;
    public UInt32 ThunRtMainProcedure;
    public Byte B3;
    public Byte B4;
    public Int16 LanguageDllId;
    public Byte B5;
    public Byte B6;
    public Byte B7;
    public Byte B8;
    public Byte B9;
    public Byte B10;
    public Byte B11;
    public Byte B12;
    public Byte FormCount;
    public Byte B13;
    public UInt32 LAddress2;
    public UInt32 LAddress3;
    public UInt32 ThunRtProject;
    public UInt32 LAddress5;  // Long
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
    public Byte[] Ba;
    public UInt32 LAddress6;
}