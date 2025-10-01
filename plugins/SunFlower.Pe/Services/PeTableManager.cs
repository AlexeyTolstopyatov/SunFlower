using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Abstractions;
using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeTableManager(PeImageModel model) : IManager
{
    public List<string> GeneralStrings { get; set; } = [];
    public List<DataTable> Results { get; set; } = [];
    public bool Is64Bit { get; } = (model.FileHeader.Characteristics & 0x100) == 0;
    
    public List<Region> Regions { get; set; } = [];
    
    public void Initialize()
    {
        MakeCharacteristics();   
        MakeHeadersTables();
        MakeSectionsTables();
        MakeExports();
        MakeImports();
        if (model.CorHeader.SizeOfHead == 0x48)
            MakeCor20Header();
        
        MakeVbClassicMoreInfo();
        MakeVb4MoreInfo();
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

    private void MakeVb4MoreInfo()
    {
        if (model.Vb4Header.ExeNameLength == 0)
            return; // never 0
        
        var head = "### `Visual Basic 4.0` Runtime Section";
        var content = 
            "This is a section that bases on the legacy by `VBGamer 45` and `DoDi` placed here." + 
            "I'm trying to demangle and define other undocumented leaked structure fields. for PE32 linked programs" +
            "So, If you see this section - you must know this is a very rare artifact.";

        var vb4 = new DataTable()
        {
            Columns =
            {
                FlowerReport.ForColumnWith("Field", "?"),
                FlowerReport.ForColumnWith("Value", "?"),
            }
        };

        vb4.Rows.Add(FlowerReport.ForColumn("Magic?", typeof(string)),
            FlowerReport.SafeString(new string(model.Vb4Header.Signature)));
        vb4.Rows.Add(FlowerReport.ForColumn("?_1", typeof(ushort)), "0x" + model.Vb4Header.Undefined1.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_2", typeof(ushort)), "0x" + model.Vb4Header.Undefined2.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_3", typeof(ushort)), "0x" + model.Vb4Header.Undefined3.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_4", typeof(ushort)), "0x" + model.Vb4Header.Undefined4.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_5", typeof(ushort)), "0x" + model.Vb4Header.Undefined5.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_6", typeof(ushort)), "0x" + model.Vb4Header.Undefined6.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_7", typeof(ushort)), "0x" + model.Vb4Header.Undefined7.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_8", typeof(ushort)), "0x" + model.Vb4Header.Undefined8.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_9", typeof(ushort)), "0x" + model.Vb4Header.Undefined9.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_10",typeof(ushort)), "0x" + model.Vb4Header.Undefined10.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_11",typeof(ushort)), "0x" + model.Vb4Header.Undefined11.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_12",typeof(ushort)), "0x" + model.Vb4Header.Undefined12.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_13",typeof(ushort)), "0x" + model.Vb4Header.Undefined13.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_14",typeof(ushort)), "0x" + model.Vb4Header.Undefined14.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_15",typeof(ushort)), "0x" + model.Vb4Header.Undefined15.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("LanguageDLLId", typeof(ushort)),
            "0x" + model.Vb4Header.LanguageDllId.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_16",typeof(ushort)), "0x" + model.Vb4Header.Undefined16.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_17",typeof(ushort)), "0x" + model.Vb4Header.Undefined17.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_18",typeof(ushort)), "0x" + model.Vb4Header.Undefined18.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("SubMainAddress", typeof(uint)),
            "0x" + model.Vb4Header.SubMainAddress.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("Address", typeof(uint)),
            "0x" + model.Vb4Header.Address.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_21",typeof(ushort)), "0x" + model.Vb4Header.Undefined21.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_22",typeof(ushort)), "0x" + model.Vb4Header.Undefined22.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_23",typeof(ushort)), "0x" + model.Vb4Header.Undefined23.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_24",typeof(ushort)), "0x" + model.Vb4Header.Undefined24.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_25",typeof(ushort)), "0x" + model.Vb4Header.Undefined25.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_26",typeof(ushort)), "0x" + model.Vb4Header.Undefined26.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExeNameLength", typeof(ushort)),
            "0x" + model.Vb4Header.ExeNameLength.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ProjNameLength", typeof(ushort)),
            "0x" + model.Vb4Header.ProjectNameLength.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("FormsCount", typeof(ushort)),
            "0x" + model.Vb4Header.FormsCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ModulesClassesCount", typeof(ushort)), "0x" + model.Vb4Header.ModulesClassesCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExternalControlsCount", typeof(ushort)), "0x" + model.Vb4Header.ExternComponentsCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("Foreach file equals 0x176d", typeof(ushort)),
            "0x" + model.Vb4Header.InEachFile176d.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("GuiTableOffset", typeof(uint)),
            "0x" + model.Vb4Header.GuiTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("???_TableOffset", typeof(uint)),
            "0x" + model.Vb4Header.UndefinedTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExternalControlsTableOffset", typeof(uint)),
            "0x" + model.Vb4Header.ExternComponentTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ProjInfoTableOffset", typeof(uint)),
            "0x" + model.Vb4Header.ProjectInfoTableOffset.ToString("X"));
        
        Regions.Add(new Region(head, content, vb4));
    }
    private void MakeClrMoreInfo()
    {
        // if image has CLR head -> this is a .NET image.
        // use Assembly instance for more details.
    }

    private void MakeVbClassicMoreInfo()
    {
        if (model.Vb5Header.ProjectExeNameOffset == 0) // never 0
            return; // take first from VB5! avoid casting
        
        // CAUGHT!
        var head = "### `Visual Basic 5.0/6.0` Runtime Section";
        var content = new StringBuilder();

        content.AppendLine("If you see this section - this is already PE32 linked binary with embedded Microsoft Visual Basic runtime.");
        content.AppendLine("This structure is a part of VB 5.0 or a VB 6.0 runtime. It depends on target DLL which `@100` requires to correct run.");
        content.AppendLine($" - VBVM ver. `{model.Vb5Header.RuntimeBuild}.{model.Vb5Header.RuntimeRevision}`");
        content.AppendLine($" - VBVM DLL: {FlowerReport.SafeString(new string(model.Vb5Header.LanguageDll))}");
        
        var vb = new DataTable()
        {
            Columns =
            {
                FlowerReport.ForColumnWith("Segment", "?"),
                FlowerReport.ForColumnWith("Value", "?")
            }
        };

        vb.Rows.Add(nameof(Vb5Header.VbMagic), FlowerReport.SafeString(new string(model.Vb5Header.VbMagic)));
        vb.Rows.Add(nameof(Vb5Header.RuntimeBuild), "0x" + model.Vb5Header.RuntimeBuild.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.LanguageDll), FlowerReport.SafeString(new string(model.Vb5Header.LanguageDll)));
        vb.Rows.Add(nameof(Vb5Header.SecondLanguageDll),
            FlowerReport.SafeString(new string(model.Vb5Header.SecondLanguageDll)));
        vb.Rows.Add(nameof(Vb5Header.RuntimeRevision), "0x" + model.Vb5Header.RuntimeRevision.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.LanguageId), "0x" + model.Vb5Header.LanguageId.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.SubMainAddress), "0x" + model.Vb5Header.SubMainAddress.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectDataPointer), "0x" + model.Vb5Header.ProjectDataPointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ControlsFlagLow), "0x" + model.Vb5Header.ControlsFlagLow.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ControlsFlagHigh), "0x" + model.Vb5Header.ControlsFlagHigh.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThreadFlags), "0x" + model.Vb5Header.ThreadFlags.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThreadCount), "0x" + model.Vb5Header.ThreadCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.FormCtlsCount), "0x" + model.Vb5Header.FormCtlsCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ExternalCtlsCount), "0x" + model.Vb5Header.ExternalCtlsCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThunkCount), "0x" + model.Vb5Header.ThunkCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.GuiTablePointer), "0x" + model.Vb5Header.GuiTablePointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ExternalTablePointer), "0x" + model.Vb5Header.ExternalTablePointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ComRegisterDataPointer), "0x" + model.Vb5Header.ComRegisterDataPointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectDescriptionOffset), "0x" + model.Vb5Header.ProjectDescriptionOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectExeNameOffset), "0x" + model.Vb5Header.ProjectExeNameOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectHelpOffset), "0x" + model.Vb5Header.ProjectHelpOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectNameOffset), "0x" + model.Vb5Header.ProjectNameOffset.ToString("X"));
        
        Regions.Add(new Region(head, content.ToString(), vb));
    }
    private void MakeHeadersTables()
    {
        var dosHeader = MakeDosHeader();
        var fileHeader = MakeFileHeader();
        var optionalHeader = Is64Bit
            ? MakeOptionalHeader()
            : MakeOptional32Header();
        
        List<DataTable> dts = [dosHeader, fileHeader, optionalHeader];
        
        if (model.CorHeader.SizeOfHead == 0x48) // <-- always true if .NET
        {
            var cor = MakeCor20Header();
            dts.Add(cor);
        }

        Results.AddRange(dts);
    }

    private DataTable MakeDosHeader()
    {
        var mz = model.MzHeader;
        DataTable table = new()
        {
            TableName = "DOS/2 Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.e_sign), "0x" + mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_cblp), "0x" + mz.e_cblp.ToString("X"));
        table.Rows.Add(nameof(mz.e_lb), "0x" + mz.e_lb.ToString("X"));
        table.Rows.Add(nameof(mz.e_relc), "0x" + mz.e_relc.ToString("X"));
        table.Rows.Add(nameof(mz.e_pars), "0x" + mz.e_pars.ToString("X"));
        table.Rows.Add(nameof(mz.e_minep), "0x" + mz.e_minep.ToString("X"));
        table.Rows.Add(nameof(mz.e_maxep), "0x" + mz.e_maxep.ToString("X"));
        table.Rows.Add(nameof(mz.ss), "0x" + mz.ss.ToString("X"));
        table.Rows.Add(nameof(mz.sp), "0x" + mz.sp.ToString("X"));
        table.Rows.Add(nameof(mz.e_crc), "0x" + mz.e_crc.ToString("X"));
        table.Rows.Add(nameof(mz.ip), "0x" + mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), "0x" + mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfarlc), "0x" + mz.e_lfarlc.ToString("X"));
        table.Rows.Add(nameof(mz.e_ovno), "0x" + mz.e_ovno.ToString("X"));
        table.Rows.Add(nameof(mz.e_oemid), "0x" + mz.e_oemid.ToString("X"));
        table.Rows.Add(nameof(mz.e_oeminfo), "0x" + mz.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfanew), "0x" + mz.e_lfanew.ToString("X"));
        
        return table;
    }

    private DataTable MakeFileHeader()
    {
        var mz = model.FileHeader;
        DataTable table = new()
        {
            TableName = "Portable Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.Machine), "0x" + mz.Machine.ToString("X"));
        table.Rows.Add(nameof(mz.NumberOfSections), "0x" + mz.NumberOfSections.ToString("X"));
        table.Rows.Add(nameof(mz.TimeDateStamp), "0x" + mz.TimeDateStamp.ToString("X"));
        table.Rows.Add(nameof(mz.PointerToSymbolTable), "0x" + mz.PointerToSymbolTable.ToString("X"));
        table.Rows.Add(nameof(mz.NumberOfSymbols), "0x" + mz.NumberOfSymbols.ToString("X"));
        table.Rows.Add(nameof(mz.SizeOfOptionalHeader), "0x" + mz.SizeOfOptionalHeader.ToString("X"));
        table.Rows.Add(nameof(mz.Characteristics), "0x" + mz.Characteristics.ToString("X"));
        
        return table;
    }

    private DataTable MakeOptionalHeader()
    {
        var optional = model.OptionalHeader;
        DataTable table = new()
        {
            TableName = "Optional Part (64-bit fields)"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(optional.Magic), "0x" + optional.Magic.ToString("X"));
        table.Rows.Add(nameof(optional.MajorLinkerVersion), "0x" + optional.MajorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorLinkerVersion), "0x" + optional.MinorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfCode), "0x" + optional.SizeOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfInitializedData), "0x" + optional.SizeOfInitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfUninitializedData), "0x" + optional.SizeOfUninitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.AddressOfEntryPoint), "0x" + optional.AddressOfEntryPoint.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfCode), "0x" + optional.BaseOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfData), "0x" + optional.BaseOfData.ToString("X"));
        table.Rows.Add(nameof(optional.ImageBase), "0x" + optional.ImageBase.ToString("X"));
        table.Rows.Add(nameof(optional.SectionAlignment), "0x" + optional.SectionAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.FileAlignment), "0x" + optional.FileAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.MajorOperatingSystemVersion), "0x" + optional.MajorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorOperatingSystemVersion), "0x" + optional.MinorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorSubsystemVersion), "0x" + optional.MajorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorSubsystemVersion), "0x" + optional.MinorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorImageVersion), "0x" + optional.MajorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorImageVersion), "0x" + optional.MinorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.Win32VersionValue), "0x" + optional.Win32VersionValue.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfImage), "0x" + optional.SizeOfImage.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeaders), "0x" + optional.SizeOfHeaders.ToString("X"));
        table.Rows.Add(nameof(optional.CheckSum), "0x" + optional.CheckSum.ToString("X"));
        table.Rows.Add(nameof(optional.Subsystem), "0x" + optional.Subsystem.ToString("X"));
        table.Rows.Add(nameof(optional.DllCharacteristics), "0x" + optional.DllCharacteristics.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackReserve), "0x" + optional.SizeOfStackReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackCommit), "0x" + optional.SizeOfStackCommit.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapReserve), "0x" + optional.SizeOfHeapReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapCommit), "0x" + optional.SizeOfHeapCommit.ToString("X"));
        table.Rows.Add(nameof(optional.LoaderFlags), "0x" + optional.LoaderFlags.ToString("X"));
        table.Rows.Add(nameof(optional.NumberOfRvaAndSizes), "0x" + optional.NumberOfRvaAndSizes.ToString("X"));
        
        return table;
    }

    private DataTable MakeOptional32Header()
    {
        var optional = model.OptionalHeader32;
        DataTable table = new()
        {
            TableName = "Optional Part (32-bit fields)"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(optional.Magic), "0x" + optional.Magic.ToString("X"));
        table.Rows.Add(nameof(optional.MajorLinkerVersion), "0x" + optional.MajorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorLinkerVersion), "0x" + optional.MinorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfCode), "0x" + optional.SizeOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfInitializedData), "0x" + optional.SizeOfInitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfUninitializedData), "0x" + optional.SizeOfUninitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.AddressOfEntryPoint), "0x" + optional.AddressOfEntryPoint.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfCode), "0x" + optional.BaseOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfData), "0x" + optional.BaseOfData.ToString("X"));
        table.Rows.Add(nameof(optional.ImageBase), "0x" + optional.ImageBase.ToString("X"));
        table.Rows.Add(nameof(optional.SectionAlignment), "0x" + optional.SectionAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.FileAlignment), "0x" + optional.FileAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.MajorOperatingSystemVersion), "0x" + optional.MajorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorOperatingSystemVersion), "0x" + optional.MinorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorSubsystemVersion), "0x" + optional.MajorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorSubsystemVersion), "0x" + optional.MinorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorImageVersion), "0x" + optional.MajorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorImageVersion), "0x" + optional.MinorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.Win32VersionValue), "0x" + optional.Win32VersionValue.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfImage), "0x" + optional.SizeOfImage.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeaders), "0x" + optional.SizeOfHeaders.ToString("X"));
        table.Rows.Add(nameof(optional.CheckSum), "0x" + optional.CheckSum.ToString("X"));
        table.Rows.Add(nameof(optional.Subsystem), "0x" + optional.Subsystem.ToString("X"));
        table.Rows.Add(nameof(optional.DllCharacteristics), "0x" + optional.DllCharacteristics.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackReserve), "0x" + optional.SizeOfStackReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackCommit), "0x" + optional.SizeOfStackCommit.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapReserve), "0x" + optional.SizeOfHeapReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapCommit), "0x" + optional.SizeOfHeapCommit.ToString("X"));
        table.Rows.Add(nameof(optional.LoaderFlags), "0x" + optional.LoaderFlags.ToString("X"));
        table.Rows.Add(nameof(optional.NumberOfRvaAndSizes), "0x" + optional.NumberOfRvaAndSizes.ToString("X"));
        
        return table;
    }

    private DataTable MakeCor20Header()
    {
        DataTable table = new()
        {
            Columns = { "Segment", "Value" },
            TableName = "CLR Part"
        };

        table.Rows.Add(nameof(model.CorHeader.SizeOfHead), "0x" + model.CorHeader.SizeOfHead.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MajorRuntimeVersion), "0x" + model.CorHeader.MajorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MinorRuntimeVersion), "0x" + model.CorHeader.MinorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MetaDataOffset), "0x" + model.CorHeader.MetaDataOffset.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.LinkerFlags), "0x" + model.CorHeader.LinkerFlags.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.EntryPointRva), "0x" + model.CorHeader.EntryPointRva.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.EntryPointToken), "0x" + model.CorHeader.EntryPointToken.ToString("X"));
        
        return table;
    }
    public void MakeSectionsTables()
    {
        DataTable sections = new()
        {
            TableName = "Sections Summary",
            Columns = 
            {
                "Name:s", 
                "VirtualAddress:4",
                "VirtualSize:4",
                "SizeOfRawData:4",
                "*RawData:4", // Pointer => *
                "*Relocs:4",
                "*Line#:4",
                "#Relocs:2",
                "#LineNumbers:2",
                "Characteristics:4" 
            }
        };
        
        foreach (var dump in model.Sections)
        {
            sections.Rows.Add(
                new String(dump.Name.Where(x => x != '\0').ToArray()),
                "0x" + dump.VirtualAddress.ToString("X"),
                "0x" + dump.VirtualSize.ToString("X"),
                "0x" + dump.SizeOfRawData.ToString("X"),
                "0x" + dump.PointerToRawData.ToString("X"),
                "0x" + dump.PointerToRelocations.ToString("X"),
                "0x" + dump.PointerToLinenumbers.ToString("X"),
                "0x" + dump.NumberOfRelocations.ToString("X"),
                "0x" + dump.NumberOfLinenumbers.ToString("X"),
                "0x" + dump.Characteristics.ToString("X")
            );
        }

        var content = "Each row of the section table is, in effect, a section header. " +
                      "This table immediately follows the optional header, if any. " +
                      "This positioning is required because the file header does not contain a direct pointer to the section table. " +
                      "Instead, the location of the section table is determined by calculating the location of the first byte after " +
                      "the headers. Make sure to use the size of the optional header as specified in the file header.\n\n" +
                      "The number of entries in the section table is given by the `NumberOfSections` field in the file header. " +
                      "Entries in the section table are numbered starting from one. " +
                      "The code and data memory section entries are in the order chosen by the linker.";
        
        Regions.Add(new Region("### Sections Table", content, sections));
    }
    
    private void MakeExternToolChain()
    {
        
    }
    
    private void MakeExports()
    {
        if (model.ExportTableModel.Functions.Count == 0)
            return;
        
        
        DataTable exports = new()
        {
            TableName = "Export Directory",
            Columns = { "Name:s", "Value:?" }
        };

        exports.Rows.Add("Name", model.ExportTableModel.ExportDirectory.Name);
        exports.Rows.Add("MajorVersion", model.ExportTableModel.ExportDirectory.MajorVersion);
        exports.Rows.Add("MinorVersion", model.ExportTableModel.ExportDirectory.MinorVersion);
        exports.Rows.Add("Base", model.ExportTableModel.ExportDirectory.Base);
        exports.Rows.Add("NamesAddress", model.ExportTableModel.ExportDirectory.AddressOfNames);
        exports.Rows.Add("ProceduresAddress", model.ExportTableModel.ExportDirectory.AddressOfFunctions);
        exports.Rows.Add("Names#", model.ExportTableModel.ExportDirectory.NumberOfNames);
        exports.Rows.Add("Procedures#", model.ExportTableModel.ExportDirectory.NumberOfFunctions);
        exports.Rows.Add("NameOrdinalsAddress", model.ExportTableModel.ExportDirectory.AddressOfNameOrdinals);
        exports.Rows.Add("TimeStamp", model.ExportTableModel.ExportDirectory.TimeDateStamp);

        DataTable functions = new()
        {
            TableName = "Exporting Functions",
            Columns = { "Name:s", "Ordinal:2", "Address:8" }
        };
        
        foreach (var function in model.ExportTableModel.Functions)
        {
            functions.Rows.Add(
                function.Name,
                "@" + function.Ordinal,
                function.Address.ToString("X")
            );
        }

        var dirContent =
            "The export symbol information begins with the export directory table, " +
            "which describes the remainder of the export symbol information. " +
            "The export directory table contains address information that is used to resolve imports to the " +
            "entry points within this image.";

        var procContent = "The export name table contains the actual string data that was pointed to by the export name pointer table. " +
                          "The strings in this table are public names that other images can use to import the symbols. " +
                          "These public export names are not necessarily the same as the private symbol names that " +
                          "the symbols have in their own image file and source code, although they can be.\n\n" +
                          "Every exported symbol has an ordinal value, which is just the index into the export address table. " +
                          "Use of export names, however, is optional. Some, all, or none of the exported symbols can have export names. " +
                          "For exported symbols that do have export names, corresponding entries in the export name pointer table and export ordinal table " +
                          "work together to associate each name with an ordinal.";
        
        Regions.Add(new Region("### Exports Directory", dirContent, exports));
        Regions.Add(new Region("### Exporting Procedures", procContent, functions));
    }

    private void MakeImports()
    {
        if (model.ImportTableModel.Modules.Count == 0)
            return;
        
        DataTable imports = new()
        {
            TableName = "Import Names Summary",
            Columns =
            {
                "Module:s",
                "Procedure:s",
                "Ordinal:2",
                "Hint:2",
                "Address:8"
            }
        };

        foreach (var import in model.ImportTableModel.Modules)
        {
            foreach (var function in import.Functions)
            {
                imports.Rows.Add(
                    import.DllName,
                    function.Name,
                    "@" + function.Ordinal,
                    "0x" + function.Hint.ToString("X4"),
                    "0x" + function.Address.ToString("X16")
                );
            }
        }

        var content =
            "Usually the import information begins with the import directory table, " +
            "which describes the remainder of the import information. " +
            "The import directory table contains address information that is used to resolve fixup references " +
            "to the entry points within a DLL image. The import directory table consists of an array of import directory entries, " +
            "one entry for each DLL to which the image refers. " +
            "The last directory entry is empty (filled with null values), " +
            "which indicates the end of the directory table.";
        
        Regions.Add(new Region("### Statically Importing Procedures", content, imports));
        
    }
}