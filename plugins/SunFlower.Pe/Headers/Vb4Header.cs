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

[StructLayout(LayoutKind.Sequential)]
public struct Vb4Header
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public Char[] Signature;
    public UInt16 CompilerVersion;
    public UInt16 Undefined1;
    public UInt16 Undefined2;
    public UInt16 Undefined3;
    public UInt16 Undefined4;
    public UInt16 Undefined5;
    public UInt16 Undefined6;
    public UInt16 Undefined7;
    public UInt16 Undefined8;
    public UInt16 Undefined9;
    public UInt16 Undefined10;
    public UInt16 Undefined11;
    public UInt16 Undefined12;
    public UInt16 Undefined13;
    public UInt16 Undefined14;
    public UInt16 Undefined15;
    public UInt16 LanguageDllId;
    public UInt16 Undefined16;
    public UInt16 Undefined17;
    public UInt16 Undefined18;
    public UInt32 SubMainAddress;
    public UInt32 Address;
    public UInt16 Undefined21;
    public UInt16 Undefined22;
    public UInt16 Undefined23;
    public UInt16 Undefined24;
    public UInt16 Undefined25;
    public UInt16 Undefined26;
    public UInt16 ExeNameLength;
    public UInt16 ProjectNameLength;
    public UInt16 FormsCount;
    public UInt16 ModulesClassesCount;
    public UInt16 ExternComponentsCount;
    public UInt16 InEachFile176d;
    public UInt32 GuiTableOffset;
    public UInt32 UndefinedTableOffset;
    public UInt32 ExternComponentTableOffset; // not 100% sure
    public UInt32 ProjectInfoTableOffset;
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