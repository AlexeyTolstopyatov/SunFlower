using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace SunFlower.Ne.Services;

public class NeTableManager
{
    private readonly NeDumpManager _manager;
    public NeTableManager(NeDumpManager manager)
    {
        _manager = manager;
        MakeCharacteristics();
        
        Regions.Add(new MzStructVisualizer(manager.MzHeader).ToRegion());
        Regions.Add(new NeStructVisualizer(manager.NeHeader).ToRegion());
        Regions.Add(new NeSegmentsVisualizer(manager.Segments).ToRegion());
        foreach (var segment in manager.Segments.Where(s => s.Relocations.Count > 0).Distinct())
        {
            Regions.Add(new NeSegmentRelocationsVisualization(segment).ToRegion());
        }
        var index = 1;
        foreach (var bundle in _manager.EntryBundles)
        {
            Regions.Add(new NeEntryBundleVisualizer(bundle, index).ToRegion());
            ++index;
        }
        
        if (manager.ResidentNames.Count > 0)
            Regions.Add(new NeNamesVisualizer(manager.ResidentNames, true).ToRegion());
        if (manager.NonResidentNames.Count > 0)
            Regions.Add(new NeNamesVisualizer(manager.NonResidentNames, false).ToRegion());
        if (manager.ModuleReferences.Count > 0)
            Regions.Add(new NeModuleReferencesVisualizer(manager.ModuleReferences).ToRegion());
        if (manager.ImportModels.Count > 0)
            Regions.Add(new NeImportsVisualizer(manager.ImportModels).ToRegion());
        
    }
    public string[] Characteristics { get; private set; } = [];
    public List<Region> Regions { get; } = [];

    private void MakeCharacteristics()
    {
        List<string> md = [];
        
        //md.Add("_Main information details took from Windows New segmented EXE header (called `IMAGE_OS2_HEADER` in Win32 API)_\r\n");
        md.Add("\r\n# Image");
        md.Add($"Project Name: {FlowerReport.SafeString(_manager.ResidentNames[0].String)}"); // <-- first name always project-name
        md.Add($"Description: {FlowerReport.SafeString(_manager.NonResidentNames[0].String)}");
        
        var os = _manager.NeHeader.NE_OS switch
        {
            0x0 => "Any OS supported", // set for *.FON. means "any OS supported"  
            0x1 => "OS/2",
            0x2 => "Windows/286",
            0x3 => "DOS/4",
            0x4 => "Windows/386",
            0x5 => "BoSS",
            _ => $"Unknown 0x{_manager.NeHeader.NE_OS:X}" // <-- really don't know how handle it
        };

        var cpu = _manager.NeHeader.NE_Flags switch
        {
            var f when (f & 0x4) != 0 => "I8086",
            var f when (f & 0x5) != 0 => "I286",
            var f when (f & 0x6) != 0 => "I386",
            var f when (f & 0x7) != 0 => "I8087",
            _ => "Not specified"
        };
        
        md.Add("\r\n### Hardware/Software");
        md.Add($" - Operating system: `{os}`");
        md.Add($" - CPU architecture: `{cpu}`");
        md.Add($" - LINK.EXE version: {_manager.NeHeader.NE_LinkerVersion}.{_manager.NeHeader.NE_LinkerRevision}");
        if (_manager.NeHeader.NE_LinkerVersion < 5)
            md.Add("> [!WARNING]\r\n>LINK.EXE 4.0 and earlier has another logic for EntryPoints table. You have a risk of wrong bytes interpretation");
        
        if (_manager.NeHeader.NE_WindowsVersionMajor > 0)
            md.Add($" - Microsoft Windows version: {_manager.NeHeader.NE_WindowsVersionMajor}.{_manager.NeHeader.NE_WindowsVersionMinor}");
        
        md.Add("\r\n## Loader requirements");
        
        md.Add($" - Heap=`{_manager.NeHeader.NE_Heap:X4}`");
        md.Add($" - Stack=`{_manager.NeHeader.NE_Stack:X4}`");
        md.Add($" - Swap area=`{_manager.NeHeader.NE_SwapArea:X4}`");
        
        md.Add($" - DOS/2 `CS:IP`={FlowerReport.FarHexString(_manager.MzHeader.cs, _manager.MzHeader.ip, true)}");
        md.Add($" - DOS/2 `SS:SP`={FlowerReport.FarHexString(_manager.MzHeader.ss, _manager.MzHeader.sp, true)}");
        
        var cs = _manager.NeHeader.NE_CsIp >> 16;
        var ip = _manager.NeHeader.NE_CsIp & 0xFFFF;
        var ss = _manager.NeHeader.NE_SsSp >> 16;
        var sp = _manager.NeHeader.NE_SsSp & 0xFFFF;
        
        md.Add($" - Win16-OS/2 `CS:IP`={FlowerReport.FarHexString((ushort)cs, (ushort)ip, true)}"); // <-- handle it
        md.Add($" - Win16-OS/2 `SS:SP`={FlowerReport.FarHexString((ushort)ss, (ushort)sp, true)}"); // <-- handle it
        md.Add($"> [!TIP]\r\n> Segmented EXE Header holds on relative EntryPoint address.\r\n> EntryPoint stores in [#{cs}](decimal) segment with 0x{ip:X} offset");
        
        md.Add("\r\n## Entities summary");
        md.Add($"1. Number of Segments - `{_manager.NeHeader.NE_SegmentsCount}`");
        md.Add($"2. Number of Entry Bundles - `{_manager.NeHeader.NE_EntriesCount}`");
        md.Add($"3. Number of Moveable Entries - `{_manager.NeHeader.NE_MovableEntriesCount}`");
        md.Add($"4. Number of Automatic Data segments - `{_manager.NeHeader.NE_AutoSegment}`");
        md.Add($"5. Number of Resources - `{_manager.NeHeader.NE_ResourcesCount}`");
        md.Add($"6. Number of `BYTE`s in NonResident names table - `{_manager.NeHeader.NE_NonResidentNamesCount}`");
        md.Add($"7. Number of Module References - `{_manager.NeHeader.NE_ModReferencesCount}`");
        
        // program flags 
        var p = _manager.NeHeader.NE_Flags;
        md.Add($"## Program Flags");
        md.Add("### How data is handled?");
        md.Add(@"
In 16-bit DOS/Windows terminology, `DGROUP` is a segment class that referring
to segments that are used for data.

Win16 used segmentation to permit a DLL or program to have multiple
instances along with an instance handle and manage multiple data
segments. In example: allowed one `NOTEPAD.EXE` code segment to execute
multiple instances of the notepad application.");
        if ((p & 0x0000) != 0) md.Add(" - `NO_AUTODATA`");
        if ((p & 0x0002) != 0) md.Add(" - `SINGLE_DATA` (shared among instances of the same program)");
        if ((p & 0x2000) != 0) md.Add(" - `MULTIPLE_DATA` (separate for each instance of the same program)");
        
        md.Add("### How application runs?");
        if ((p & 0x0008) != 0) md.Add(" - `PROTECTED_MODE_ONLY`");
        if ((p & 0x0004) != 0) md.Add(" - `GLOBINIT` - (global initialization)");
        
        md.Add("### Extra details?");
        if ((p & 0x2000) != 0) md.Add(" - `LINK_ERR` - (module has errors after linkage. Don't try to run it)");
        if ((p & 0x8000) != 0) md.Add(" - `LIB_MODULE` (dynamically linked module)");
        
        md.Add("## Application Flags");
        md.Add("This block (field) tells how windowing or not windowing wants to run");
        
        var a = _manager.NeHeader.NE_Flags;
        
        if ((a & 0x0080) != 0) md.Add(" - `OS2_FAMILY` (OS/2 family application. You can see OS/2 flags section)");
        if ((a & 0x0020) != 0) md.Add(" - `IMAGE_ERR` (OS doesn't want that you run it).");
        if ((a & 0x0040) != 0) md.Add(" - `NON_CONFORM` (nonconforming program)");
        
        if (_manager.NeHeader.NE_FlagOthers != 0)
        {
            md.Add("## OS/2 Flags");
            md.Add("Sunflower plugin shows this section if `e_flagothers` not zero. But I also suppose if appflags has `OS2_FAMILY`" +
                   " or `e_os` equals 0x1, what means OS/2 - you can read this section.");
            var o = _manager.NeHeader.NE_FlagOthers;
            if ((o & 0x0001) != 0) md.Add(" - `LONG_NAMES` (avoid FAT rule 8.3 convertion)");
            if ((o & 0x0002) != 0) md.Add(" - `PROTECTED_MODE` (OS/2 2.0+ protected mode application)");
            if ((o & 0x0004) != 0) md.Add(" - `PROP_FONTS` (proportional fonts)");
            if ((o & 0x0008) != 0) md.Add(" - `GANGLOAD_AREA`");
        }
        
        Characteristics = md.ToArray();
    }
}