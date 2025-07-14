using System.Data;
using System.Security.Cryptography.X509Certificates;
using Sunflower.Links.Headers;

namespace SunFlower.Links.Services;

public class PifTableManager
{
    private PifDumpManager _manager;

    public DataTable MicrosoftPifExTable { get; private set; } = new();
    public DataColumn Windows386Table { get; private set; } = new();

    public PifTableManager(PifDumpManager manager)
    {
        _manager = manager;
        MakeMicrosoftPifEx();
    }

    private void MakeMicrosoftPifEx()
    {
        DataTable table = new("Microsoft PIFex Section")
        {
            Columns =
            {
                "Segment", 
                "Value"
            }
        };

        table.Rows.Add(nameof(MicrosoftPifEx.Zero), _manager.MicrosoftPifEx.Zero);
        table.Rows.Add(nameof(MicrosoftPifEx.Checksum), _manager.MicrosoftPifEx.Checksum.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.WindowTitle), TryExcludeSpecificAscii(_manager.MicrosoftPifEx.WindowTitle).Trim('\0'));
        table.Rows.Add(nameof(MicrosoftPifEx.ConventionalMemMaxSizeK), _manager.MicrosoftPifEx.ConventionalMemMaxSizeK.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.ConventionalMemMinSizeK), _manager.MicrosoftPifEx.ConventionalMemMinSizeK.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.FileName), TryExcludeSpecificAscii(_manager.MicrosoftPifEx.FileName).Trim('\0'));

        string fileDosFlags = string.Empty;
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0001) != 0)
            fileDosFlags += "`Directly Modify Memory`,";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0002) != 0)
            fileDosFlags += "`Graphic Mode`,";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0004) != 0)
            fileDosFlags += "`Prevent Program Switch`,";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0008) != 0)
            fileDosFlags += "`No screen exchange`,";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0010) != 0)
            fileDosFlags += "`Close Window on Exit`";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0020) != 0)
            fileDosFlags += "`Direct interaction COM1`";
        if ((_manager.MicrosoftPifEx.FileDosFlags & 0x0040) != 0)
            fileDosFlags += "`Direct interaction COM2`";
        
        table.Rows.Add(nameof(MicrosoftPifEx.FileDosFlags), fileDosFlags);
        table.Rows.Add(nameof(MicrosoftPifEx.WorkingDirectory), TryExcludeSpecificAscii(_manager.MicrosoftPifEx.WorkingDirectory));
        table.Rows.Add(nameof(MicrosoftPifEx.ArgumentsVector), TryExcludeSpecificAscii(_manager.MicrosoftPifEx.ArgumentsVector));
        table.Rows.Add(nameof(MicrosoftPifEx.VideoMode), _manager.MicrosoftPifEx.VideoMode.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.VideoPagesCount), _manager.MicrosoftPifEx.VideoPagesCount.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.FirstInt), _manager.MicrosoftPifEx.FirstInt.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.LastInt), _manager.MicrosoftPifEx.LastInt.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.WindowHeight), _manager.MicrosoftPifEx.WindowHeight.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.WindowWidth), _manager.MicrosoftPifEx.WindowWidth.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.WindowPositionX), _manager.MicrosoftPifEx.WindowPositionX.ToString("X"));
        table.Rows.Add(nameof(MicrosoftPifEx.WindowPositionY), _manager.MicrosoftPifEx.WindowPositionY.ToString("X"));

        string videoMode = "";
        if ((_manager.MicrosoftPifEx.VideoPageFlags & 0x0007) != 0)
            videoMode += $"`Number of last Video Page is {_manager.MicrosoftPifEx.VideoPageFlags & 0x0007}`";
        if ((_manager.MicrosoftPifEx.VideoPageFlags & 0x0010) != 0)
            videoMode += "`Graphic Mode`";
        
        table.Rows.Add(nameof(MicrosoftPifEx.VideoPageFlags), videoMode);

        string anotherVideoFlags = "";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x0010) != 0)
            anotherVideoFlags += $"`Direct interaction KEYBOARD`";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x0020) != 0)
            anotherVideoFlags += "`Use coprocessor`";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x0040) != 0)
            anotherVideoFlags += $"`Can stop in background`";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x1000) != 0)
            anotherVideoFlags += "`Directly modify screen`";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x2000) != 0)
            anotherVideoFlags += $"`Exchange interrupt vector`";
        if ((_manager.MicrosoftPifEx.AnotherFlags & 0x4000) != 0)
            anotherVideoFlags += "`Parameters in [argv]`";

        table.Rows.Add(nameof(MicrosoftPifEx.AnotherFlags), anotherVideoFlags);

        MicrosoftPifExTable = table;
    }

    private void MakeWindows386(BinaryReader reader)
    {
        DataTable table = new("Windows 386 3.0 Section")
        {
            Columns = { "Segment", "Value" }
        };
        table.Rows.Add(nameof(_manager.Windows3X386.ConventionalMemMaxSizeK),
            _manager.Windows3X386.ConventionalMemMaxSizeK.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.ConventionalMemReqSizeK),
            _manager.Windows3X386.ConventionalMemReqSizeK.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.ActivePriority),
            _manager.Windows3X386.ActivePriority.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.BackgroundPriority),
            _manager.Windows3X386.BackgroundPriority.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.EmsMemMaxSizeK),
            _manager.Windows3X386.EmsMemMaxSizeK.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.EmsMemReqSizeK),
            _manager.Windows3X386.EmsMemReqSizeK.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.XmsMemMaxSizeK),
            _manager.Windows3X386.XmsMemMaxSizeK.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.XmsMemReqSizeK),
            _manager.Windows3X386.XmsMemReqSizeK.ToString("X"));
        
        // DosFlags
        string dosFlags = "";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000001) != 0)
            dosFlags += "Permit Exit when Active, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000002) != 0)
            dosFlags += "Continue work in background, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000004) != 0)
            dosFlags += "Exclusive mode, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000008) != 0)
            dosFlags += "Full screen mode, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000020) != 0)
            dosFlags += "Not use [Alt]+[Tab], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000040) != 0)
            dosFlags += "Not use [Alt]+[Esc], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000080) != 0)
            dosFlags += "Not use [Alt]+[Space], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000100) != 0)
            dosFlags += "Not use [Alt]+[Enter], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000200) != 0)
            dosFlags += "Not use [Alt]+[PrtScr], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000400) != 0)
            dosFlags += "Not use [PrtScr], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00000800) != 0)
            dosFlags += "Not use [Ctrl]+[Esc], ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00002000) != 0)
            dosFlags += "Not use HMA, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00004000) != 0)
            dosFlags += "Use shortcut key, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00008000) != 0)
            dosFlags += "EMS locked, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00010000) != 0)
            dosFlags += "XMS locked, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00020000) != 0)
            dosFlags += "Fast paste, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00040000) != 0)
            dosFlags += "Application memory locked, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00080000) != 0)
            dosFlags += "Protected memory, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00100000) != 0)
            dosFlags += "Minimized Window, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00200000) != 0)
            dosFlags += "Maximized Window, ";
        if ((_manager.Windows3X386.DosModeFlags & 0x00800000) != 0)
            dosFlags += "MS-DOS mode,";
        if ((_manager.Windows3X386.DosModeFlags & 0x01000000) != 0)
            dosFlags += "Prevent Windows detection,";
        if ((_manager.Windows3X386.DosModeFlags & 0x04000000) != 0)
            dosFlags += "Ask to transit MS-DOS mode,";
        if ((_manager.Windows3X386.DosModeFlags & 0x10000000) != 0)
            dosFlags += "Not warn before transition in MS-DOS mode,";
        if ((_manager.Windows3X386.DosModeFlags & 0x1000000) != 0)
            dosFlags += "EMS locked";

        table.Rows.Add(nameof(_manager.Windows3X386.DosModeFlags), dosFlags);
        
        // VideoFlags

        string videoFlags = "";
        
        if ((_manager.Windows3X386.VideoFlags & 0x0001) != 0)
            videoFlags += "Video ROM emulation, ";
        if ((_manager.Windows3X386.VideoFlags & 0x0002) != 0)
            videoFlags += "Not check ports: text., ";
        if ((_manager.Windows3X386.VideoFlags & 0x0004) != 0)
            videoFlags += "Not check ports: low graphics., ";
        if ((_manager.Windows3X386.VideoFlags & 0x0008) != 0)
            videoFlags += "Not check ports: high graphics., ";
        if ((_manager.Windows3X386.VideoFlags & 0x0010) != 0)
            videoFlags += "Video memory: text.,";
        if ((_manager.Windows3X386.VideoFlags & 0x0020) != 0)
            videoFlags += "Video memory: low graphics, ";
        if ((_manager.Windows3X386.VideoFlags & 0x0040) != 0)
            videoFlags += "Video memory: high graphics, ";
        if ((_manager.Windows3X386.VideoFlags & 0x0080) != 0)
            videoFlags += "Retain video mem.";

        table.Rows.Add(nameof(_manager.Windows3X386.VideoFlags), videoFlags);
        table.Rows.Add(nameof(_manager.Windows3X386.Reserved1),
            _manager.Windows3X386.Reserved1.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.ConventionalMemMaxSizeK),
            _manager.Windows3X386.ShortCutKeyCode.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.ConventionalMemMaxSizeK),
            _manager.Windows3X386.ShortCutKeyCode.ToString("X"));
        
        // Shortcut flags
        
        table.Rows.Add(nameof(_manager.Windows3X386.Reserved2),
            _manager.Windows3X386.Reserved2.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.Reserved3),
            _manager.Windows3X386.Reserved3.ToString("X"));
        table.Rows.Add(nameof(_manager.Windows3X386.Reserved4),
            _manager.Windows3X386.Reserved4.ToString("X"));
        
        table.Rows.Add(nameof(_manager.Windows3X386.ArgumentsVector),
            TryExcludeSpecificAscii(_manager.Windows3X386.ArgumentsVector));
    }
    
    private static string TryExcludeSpecificAscii(char[] array)
    {
        char[] excluded = array.Where(c => char.IsAsciiLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSeparator(c)).ToArray();
        return new string(excluded);
    }
}