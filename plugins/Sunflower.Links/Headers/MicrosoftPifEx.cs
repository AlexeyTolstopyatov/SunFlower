using System.Runtime.InteropServices;

namespace Sunflower.Links.Headers;

/// <summary>
/// Windows 1x PIF format. Executes for every OS
/// if next following section not exists.
///
/// In example, visual basic 1.x PIF file runs under Microsoft Windows 2.11
/// but doesn't have "WINDOWS 2.0 286" section.
/// In this case all link requirements are taking from "MICROSOFT PIFEX" section.
///
/// Microsoft PIFex section is necessary.
///
/// If start of file equals 0x171 -> <see cref="MicrosoftPifEx"/> section
/// follows directly by the header <see cref="PifSectionHead"/>
///
/// If start of file WORD more 0x171 -> Microsoft PIFex section follows
/// by this offset.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MicrosoftPifEx
{
    public byte Zero;
    public byte Checksum; // Windows 9x => should me 0x78
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public char[] WindowTitle;

    public ushort ConventionalMemMaxSizeK;
    public ushort ConventionalMemMinSizeK;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
    public char[] FileName;

    public ushort FileDosFlags;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] WorkingDirectory;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ArgumentsVector;

    public byte VideoMode;
    public byte VideoPagesCount;
    public byte FirstInt;
    public byte LastInt;
    public byte WindowWidth;
    public byte WindowHeight;
    public byte WindowPositionX;
    public byte WindowPositionY;
    public ushort VideoPageFlags;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public byte[] Alignment;

    public ushort AnotherFlags;
    
}