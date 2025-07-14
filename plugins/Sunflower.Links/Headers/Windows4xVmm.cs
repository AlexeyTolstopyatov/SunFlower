using System.Runtime.InteropServices;

namespace Sunflower.Links.Headers;

public struct Windows4xVmm
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 88)]
    public byte[] Reserved88;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
    public char[] IconStorage; // PIFMGR.DLL

    public ushort IconIndex; // PIFMRG resource following by 1033 (i.e.)
    public ushort DosModeFlags;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public char[] Reserved10;

    public ushort Priority; // changes from 0 to 100 (max 100).
    public ushort DosWindowFlags;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved8;

    public ushort NumberOfLines;
    public ushort ShortcutKeyFlags;

    public ushort Reserved00;
    public ushort Reserved05;
    public ushort Reserved19;
    public ushort Reserved03;
    public ushort ReservedC8;
    public ushort Reserved3E8;
    public ushort Reserved02;
    public ushort Reserved0A;

    public ushort MouseFlags;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] Reserved6;

    public ushort FontFlags;
    public ushort Unknown;
    public ushort CurrentFontSizeX;
    public ushort CurrentFontSizeY;
    public ushort CurrentFontSizeXToo;
    public ushort CurrentFontSizeYToo;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] RasterFontName;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] TrueTypeFontName;

    public ushort UnknownWinNt; // stays not 0 only for Windows NT

    public ushort ToolBarFlags;
    public ushort ScreenSizeX; // after start = 80
    public ushort ScreenSizeY; // after start = 25
    public ushort ClientWindowAreaX;
    public ushort ClientWindowAreaY;
    public ushort WindowSizeX;
    public ushort WindowSizeY;

    public ushort AlsoUnknown;

    public ushort RestoreFlag;
    public ushort WindowStateFlag; // if (w & 0x02) != 0 then window last state - maximized 
    public ushort WindowState; // 1 = Normal 2 = Maximized 3 = Hidden/Minimized

    public ushort UnknownWin951;
    public ushort UnknownWin952;

    public ushort RightMaxBorderPosition;
    public ushort BottomMaxBorderPosition;
    public ushort LeftMaxBorderPosition;
    public ushort TopMaxBorderPosition;

    public ushort RightBorderPosition;
    public ushort BottomBorderPosition;
    public ushort LeftBorderPosition;
    public ushort TopBorderPosition;

    public uint AnotherUnknown0; // not used at all OS!!!

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
    public char[] CmdFileName;

    public ushort EnvironmentMemAlloc;
    public ushort DpmiAlloc;

    public ushort Unknown1;
}
