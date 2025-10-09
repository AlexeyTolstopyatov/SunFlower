using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeImageView(PeImageModel model) : IManager
{
    public List<string> GeneralStrings { get; } = [];
    private bool Is64Bit { get; } = (model.FileHeader.Characteristics & 0x100) == 0;
    public List<Region> Regions { get; } = [];
    
    public void Initialize()
    {
        MakeCharacteristics();

        var mz = new MzStructVisualizer(model.MzHeader).ToRegion();
        var coff = new PeStructVisualizer(model.FileHeader).ToRegion();
        var pe = Is64Bit switch
        {
            true => new Pe64StructVisualizer(model.OptionalHeader).ToRegion(),
            false => new Pe32StructVisualizer(model.OptionalHeader32).ToRegion()
        };
        var dirs = new PeDirectoriesVisualizer(model.Directories).ToRegion();
        var sects = new PeSectionsVisualizer(model.Sections).ToRegion();
        
        // main tree of executable.
        Regions.AddRange([
            mz,
            coff,
            pe,
            dirs,
            sects
        ]);
        
        if (model.Directories[0].VirtualAddress != 0)
        {
            Regions.Add(new PeExportDirectoryVisualizer(model.ExportTableModel).ToRegion());
            Regions.Add(new PeExportsVisualizer(model.ExportTableModel.Functions).ToRegion());
        }
        
        if (model.Directories[1].VirtualAddress != 0)
            Regions.Add(new PeImportsVisualizer(model.ImportTableModel).ToRegion());
        // another way to define .NET
        if (model.CorHeader.SizeOfHead == 0x48)
            Regions.Add(new Cor20StructVisualizer(model.CorHeader).ToRegion());
        
        if (model.Vb4Header.Signature != null! && model.Vb4Header.Signature == "VB5!".ToArray())
            Regions.Add(new Vb4StructVisualizer(model.Vb4Header).ToRegion());
        
        if (model.Vb5Header.VbMagic != null!)
            Regions.Add(new Vb5StructVisualizer(model.Vb5Header).ToRegion());
        
    }

    private void MakeCharacteristics()
    {
        GeneralStrings.Add("# Image");
        GeneralStrings.Add("This image is Windows NT linked module (Portable Executable format). Project name and description temporary unable to find. No one way to tell.");
        GeneralStrings.Add("Instead New executable (`NE`) or Linear executables (`LX/LE`) formats, this one not holds standard fields about module.");
        
        // pain #1
        var osver = Is64Bit
            ? $"{model.OptionalHeader.MajorOperatingSystemVersion}.{model.OptionalHeader.MinorOperatingSystemVersion}"
            : $"{model.OptionalHeader32.MajorOperatingSystemVersion}.{model.OptionalHeader32.MinorOperatingSystemVersion}";
        var linkv = Is64Bit
            ? $"{model.OptionalHeader.MajorLinkerVersion}.{model.OptionalHeader.MinorLinkerVersion}"
            : $"{model.OptionalHeader32.MajorLinkerVersion}.{model.OptionalHeader32.MinorLinkerVersion}";
        var ssver = Is64Bit
            ? $"{model.OptionalHeader.MajorSubsystemVersion}.{model.OptionalHeader.MinorSubsystemVersion}"
            : $"{model.OptionalHeader32.MajorSubsystemVersion}.{model.OptionalHeader32.MinorSubsystemVersion}";
        var subsys = Is64Bit
            ? model.OptionalHeader.Subsystem
            : model.OptionalHeader32.Subsystem;
        var subsysStr = subsys switch
        {
            0x0001 => "`NATIVE` (Windows Kernel Driver). Tells loader to not call any subsystem client for this image.",
            0x0002 => "`WIN32_CUI` (Win32 Console Application). Tells loader, entry point is traditional `DWORD main(/*args*/)` function.`",
            0x0003 => "`WIN32_GUI` (Win32 Windowing Application). Tells loader, entry point is a `WinMain` procedure.",
            0x0005 => "`OS2_CUI` (OS/2 1.0+ Console Application). Tells loader not use Win32 modules. Just `DOSCALLS`, `NETAPI` instead.",
            0x0007 => "`POSIX_CUI` (POSIX subsystem). Tells loader fully avoid Win32, to use POSIX compatible `fork`, `signal`, ... other functions and modules instead",
            0x0008 => "`NATIVE_WIN` (Windows 9x driver or `Win32s`-linked PE). I suppose this flag tells ring-2 driver instead, like `*.DRV` NE-linked images. Or secondary I suppose, this is `Win32s` linked PE.",
            0x0009 => "`WINCE_GUI` (Windows CE GUI) Tells loader that image have Windows CE kernel specific in code. Don't try to run it under WinNT!",
            _ => "`?` (Unknown flag)",
            //0x0010 => "``"
        };
        
        GeneralStrings.Add("## Hardware/Software");
        GeneralStrings.Add(" - Target OS: `Windows NT`");
        GeneralStrings.Add($" - Expected Windows NT `{osver}`");
        GeneralStrings.Add($" - Minimum Windows NT `{ssver}`");
        GeneralStrings.Add($" - Linker `v.{linkv}`");
        
        GeneralStrings.Add("### Windows Subsystem");
        GeneralStrings.Add("Windows NT architecture includes subsystems like scopes at the userland (or ring-3)" +
                           "They are provide a support of I/O, net, GDI and other features, and PE module holds" +
                           "a value, which subsystem's client will be called for running it.");
        GeneralStrings.Add($" - `0x{subsys:X}`\n" +
                           $" - {subsysStr}");
        
        GeneralStrings.Add("### Characteristics");
        GeneralStrings.Add("File characteristics always check by loader and helps it to run application correctly.");
        
        var c = model.FileHeader.Characteristics;
        
        if ((c & 0x0001) != 0) GeneralStrings.Add(" - `image_file_relocs_stripped` Windows CE, and Windows NT and later. This indicates that the file does not contain base relocations and must therefore be loaded at its preferred base address. If the base address is not available, the loader reports an error. The default behavior of the linker is to strip base relocations from executable (EXE) files.");
        if ((c & 0x0002) != 0) GeneralStrings.Add(" - `image_file_executable` This indicates that the image file is valid and can be run. _If this flag is not set_, it indicates a linker error.");
        if ((c & 0x0004) != 0) GeneralStrings.Add(" - `image_file_linenums_stripped` COFF line numbers have been removed. This flag is deprecated and should be zero");
        if ((c & 0x0008) != 0) GeneralStrings.Add(" - `image_file_local_syms_stripped` COFF symbol table entries for local symbols have been removed. This flag is deprecated and should be zero.");
        if ((c & 0x0010) != 0) GeneralStrings.Add(" - `image_file_aggressive_ws_trim` [Obsolete]. Aggressively trim working set. This flag is deprecated for Windows 2000 and later and must be zero.");
        if ((c & 0x0020) != 0) GeneralStrings.Add(" - `image_file_large_address_aware` Application can handle > `2-GB` addresses.");
        if ((c & 0x0040) != 0) GeneralStrings.Add(" - `image_file_reserved` **Should be zero!**");
        if ((c & 0x0080) != 0) GeneralStrings.Add(" - `image_file_bytes_reverse_lo` **Little endian:** the least significant bit (LSB) precedes the most significant bit (MSB) in memory. _This flag is deprecated and should be zero_");
        if ((c & 0x0100) != 0) GeneralStrings.Add(" - `image_file_32bit_machine` Machine is based on a 32-bit-word architecture.");
        if ((c & 0x0200) != 0) GeneralStrings.Add(" - `image_debug_stripped` `.debug` section missing. Or debug information removed entirely from image.");
        if ((c & 0x0400) != 0) GeneralStrings.Add(" - `image_file_media_run_from_swap` If the image is on removable media, fully load it and copy it to the swap file.");
        if ((c & 0x0800) != 0) GeneralStrings.Add(" - `image_net_run_from_swap` If the image is on network media, fully load it and copy it to the swap file.");
        if ((c & 0x1000) != 0) GeneralStrings.Add(" - `image_file_system` The image file is a system file, not a user program.");
        if ((c & 0x2000) != 0) GeneralStrings.Add(" - `image_file_dll` The image file is a dynamic-link library (`.DLL`). Such files are considered executable files for almost all purposes, although they cannot be directly run.");
        if ((c & 0x4000) != 0) GeneralStrings.Add(" - `image_file_up_system_only` The file should be run only on a uniprocessor machine.");
        if ((c & 0x8000) != 0) GeneralStrings.Add(" - `image_file_bytes_reverse_hi` **Big endian:** the MSB precedes the LSB in memory. _This flag is deprecated and should be zero_.");

        GeneralStrings.Add("### DLL Characteristics");
        GeneralStrings.Add("Describe any PE linked module and any PE module holds `WORD DllCharacteristics` field. Not only `.DLL`s.");
        var d = Is64Bit
            ? model.OptionalHeader.DllCharacteristics
            : model.OptionalHeader32.DllCharacteristics;
        
        if ((d & 0x0020) != 0) GeneralStrings.Add("`image_dll_high_entropy_va` Image can handle a high entropy 64-bit virtual address space.");
        if ((d & 0x0040) != 0) GeneralStrings.Add("`image_dll_dynamic_base` DLL can be relocated at load time.");
        if ((d & 0x0080) != 0) GeneralStrings.Add("`image_dll_force_integrity` Code Integrity checks are enforced.");
        if ((d & 0x0100) != 0) GeneralStrings.Add("`image_dll_nx_compat` Image is NX compatible");
        if ((d & 0x0200) != 0) GeneralStrings.Add("`image_dll_no_isolation` Isolation aware, but do not isolate the image.");
        if ((d & 0x0400) != 0) GeneralStrings.Add("`image_dll_no_seh` Doesn't use structured exception (SE) handling. No SE handler may be called in this image.");
        if ((d & 0x0800) != 0) GeneralStrings.Add("`image_dll_no_bind` Don't bind the image.");
        if ((d & 0x1000) != 0) GeneralStrings.Add("`image_dll_appcontainer` Image must execute in an AppContainer.");
        if ((d & 0x2000) != 0) GeneralStrings.Add("`image_dll_wdm_driver` Image is WDM driver.");
        if ((d & 0x4000) != 0) GeneralStrings.Add("`image_dll_guard_cf` Image supports Control Flow Guard.");
        if ((d & 0x8000) != 0) GeneralStrings.Add("`image_dll_terminal_server_aware` Terminal Server aware.");

        var ip = Is64Bit 
            ? model.OptionalHeader.AddressOfEntryPoint 
            : model.OptionalHeader32.AddressOfEntryPoint;
        
        GeneralStrings.Add("### Loader requirements");
        
        GeneralStrings.Add($"> [!TIP] \n> Address of an EntryPoint for this program is `0x{ip:X8}`");
        GeneralStrings.Add("> The address of the entry point relative to the image base when the executable file is loaded into memory.");
        
        // pain #2
        // heapsize/stacksize/win32value/.../...
        
    }
}