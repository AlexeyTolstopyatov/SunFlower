using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

/*
 * This information took from "Visual Basic Image Components".pdf
 * see in repo: https://github.com/AlexeyToltopyatov/JellyBins/JellyBins.Documents/
 */
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vb5Header
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public Char[] VbMagic;
    
    public UInt16 RuntimeBuild;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
    public Char[] LanguageDll;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
    public Char[] SecondLanguageDll;
    
    public UInt16 RuntimeRevision;
    public UInt32 LanguageId;
    public UInt32 SecondLanguageId;
    public UInt32 SubMainAddress;
    public UInt32 ProjectDataPointer;
    public UInt32 ControlsFlagLow;
    public UInt32 ControlsFlagHigh;
    public UInt32 ThreadFlags;
    public UInt32 ThreadCount;
    public UInt16 FormCtlsCount;
    public UInt16 ExternalCtlsCount;
    public UInt32 ThunkCount;
    public UInt32 GuiTablePointer;
    public UInt32 ExternalTablePointer;
    public UInt32 ComRegisterDataPointer;
    public UInt32 ProjectDescriptionOffset;
    public UInt32 ProjectExeNameOffset;
    public UInt32 ProjectHelpOffset;
    public UInt32 ProjectNameOffset;
}