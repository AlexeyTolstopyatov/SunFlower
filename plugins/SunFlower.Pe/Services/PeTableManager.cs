using System.Data;
using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeTableManager(PeImageModel model) : IManager
{
    public List<DataTable> Results { get; set; } = [];
    public bool Is64Bit { get; } = (model.FileHeader.Characteristics & 0x100) == 0;

    public void Initialize()
    {
        MakeHeadersTables();
        MakeSectionsTables();
        MakeExports();
        MakeImports();
        if (model.CorHeader.SizeOfHead == 0x48)
            MakeCor20Header();
        
    }

    private void MakeHeadersTables()
    {
        DataTable dosHeader = MakeDosHeader();
        DataTable fileHeader = MakeFileHeader();
        DataTable optionalHeader = Is64Bit
            ? MakeOptionalHeader()
            : MakeOptional32Header();
        
        List<DataTable> dts = [dosHeader, fileHeader, optionalHeader];
        
        if (model.CorHeader.SizeOfHead == 0x48) // <-- always true if .NET
        {
            DataTable cor = MakeCor20Header();
            dts.Add(cor);
        }

        Results.AddRange(dts);;
    }

    private DataTable MakeDosHeader()
    {
        MzHeader mz = model.MzHeader;
        DataTable table = new()
        {
            TableName = "DOS/2 Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.e_sign), mz.e_sign.ToString("X"));
        table.Rows.Add(nameof(mz.e_lastb), mz.e_lastb.ToString("X"));
        table.Rows.Add(nameof(mz.e_fbl), mz.e_fbl.ToString("X"));
        table.Rows.Add(nameof(mz.e_relc), mz.e_relc.ToString("X"));
        table.Rows.Add(nameof(mz.e_pars), mz.e_pars.ToString("X"));
        table.Rows.Add(nameof(mz.e_minep), mz.e_minep.ToString("X"));
        table.Rows.Add(nameof(mz.e_maxep), mz.e_maxep.ToString("X"));
        table.Rows.Add(nameof(mz.ss), mz.ss.ToString("X"));
        table.Rows.Add(nameof(mz.sp), mz.sp.ToString("X"));
        table.Rows.Add(nameof(mz.e_check), mz.e_check.ToString("X"));
        table.Rows.Add(nameof(mz.ip), mz.ip.ToString("X"));
        table.Rows.Add(nameof(mz.cs), mz.cs.ToString("X"));
        table.Rows.Add(nameof(mz.e_reltableoff), mz.e_reltableoff.ToString("X"));
        table.Rows.Add(nameof(mz.e_overnum), mz.e_overnum.ToString("X"));
        table.Rows.Add(nameof(mz.e_oemid), mz.e_oemid.ToString("X"));
        table.Rows.Add(nameof(mz.e_oeminfo), mz.e_oeminfo.ToString("X"));
        table.Rows.Add(nameof(mz.e_lfanew), mz.e_lfanew.ToString("X"));
        
        return table;
    }

    private DataTable MakeFileHeader()
    {
        PeFileHeader mz = model.FileHeader;
        DataTable table = new()
        {
            TableName = "Portable Executable"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(mz.Machine), mz.Machine.ToString("X"));
        table.Rows.Add(nameof(mz.NumberOfSections), mz.NumberOfSections.ToString("X"));
        table.Rows.Add(nameof(mz.TimeDateStamp), mz.TimeDateStamp.ToString("X"));
        table.Rows.Add(nameof(mz.PointerToSymbolTable), mz.PointerToSymbolTable.ToString("X"));
        table.Rows.Add(nameof(mz.NumberOfSymbols), mz.NumberOfSymbols.ToString("X"));
        table.Rows.Add(nameof(mz.SizeOfOptionalHeader), mz.SizeOfOptionalHeader.ToString("X"));
        table.Rows.Add(nameof(mz.Characteristics), mz.Characteristics.ToString("X"));
        
        return table;
    }

    private DataTable MakeOptionalHeader()
    {
        PeOptionalHeader optional = model.OptionalHeader;
        DataTable table = new()
        {
            TableName = "Optional Part (64-bit fields)"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(optional.Magic), optional.Magic.ToString("X"));
        table.Rows.Add(nameof(optional.MajorLinkerVersion), optional.MajorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorLinkerVersion), optional.MinorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfCode), optional.SizeOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfInitializedData), optional.SizeOfInitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfUninitializedData), optional.SizeOfUninitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.AddressOfEntryPoint), optional.AddressOfEntryPoint.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfCode), optional.BaseOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfData), optional.BaseOfData.ToString("X"));
        table.Rows.Add(nameof(optional.ImageBase), optional.ImageBase.ToString("X"));
        table.Rows.Add(nameof(optional.SectionAlignment), optional.SectionAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.FileAlignment), optional.FileAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.MajorOperatingSystemVersion), optional.MajorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorOperatingSystemVersion), optional.MinorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorSubsystemVersion), optional.MajorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorSubsystemVersion), optional.MinorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorImageVersion), optional.MajorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorImageVersion), optional.MinorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.Win32VersionValue), optional.Win32VersionValue.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfImage), optional.SizeOfImage.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeaders), optional.SizeOfHeaders.ToString("X"));
        table.Rows.Add(nameof(optional.CheckSum), optional.CheckSum.ToString("X"));
        table.Rows.Add(nameof(optional.Subsystem), optional.Subsystem.ToString("X"));
        table.Rows.Add(nameof(optional.DllCharacteristics), optional.DllCharacteristics.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackReserve), optional.SizeOfStackReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackCommit), optional.SizeOfStackCommit.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapReserve), optional.SizeOfHeapReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapCommit), optional.SizeOfHeapCommit.ToString("X"));
        table.Rows.Add(nameof(optional.LoaderFlags), optional.LoaderFlags.ToString("X"));
        table.Rows.Add(nameof(optional.NumberOfRvaAndSizes), optional.NumberOfRvaAndSizes.ToString("X"));
        
        return table;
    }

    private DataTable MakeOptional32Header()
    {
        PeOptionalHeader32 optional = model.OptionalHeader32;
        DataTable table = new()
        {
            TableName = "Optional Part (32-bit fields)"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(optional.Magic), optional.Magic.ToString("X"));
        table.Rows.Add(nameof(optional.MajorLinkerVersion), optional.MajorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorLinkerVersion), optional.MinorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfCode), optional.SizeOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfInitializedData), optional.SizeOfInitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfUninitializedData), optional.SizeOfUninitializedData.ToString("X"));
        table.Rows.Add(nameof(optional.AddressOfEntryPoint), optional.AddressOfEntryPoint.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfCode), optional.BaseOfCode.ToString("X"));
        table.Rows.Add(nameof(optional.BaseOfData), optional.BaseOfData.ToString("X"));
        table.Rows.Add(nameof(optional.ImageBase), optional.ImageBase.ToString("X"));
        table.Rows.Add(nameof(optional.SectionAlignment), optional.SectionAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.FileAlignment), optional.FileAlignment.ToString("X"));
        table.Rows.Add(nameof(optional.MajorOperatingSystemVersion), optional.MajorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorOperatingSystemVersion), optional.MinorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorSubsystemVersion), optional.MajorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorSubsystemVersion), optional.MinorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MajorImageVersion), optional.MajorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.MinorImageVersion), optional.MinorImageVersion.ToString("X"));
        table.Rows.Add(nameof(optional.Win32VersionValue), optional.Win32VersionValue.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfImage), optional.SizeOfImage.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeaders), optional.SizeOfHeaders.ToString("X"));
        table.Rows.Add(nameof(optional.CheckSum), optional.CheckSum.ToString("X"));
        table.Rows.Add(nameof(optional.Subsystem), optional.Subsystem.ToString("X"));
        table.Rows.Add(nameof(optional.DllCharacteristics), optional.DllCharacteristics.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackReserve), optional.SizeOfStackReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfStackCommit), optional.SizeOfStackCommit.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapReserve), optional.SizeOfHeapReserve.ToString("X"));
        table.Rows.Add(nameof(optional.SizeOfHeapCommit), optional.SizeOfHeapCommit.ToString("X"));
        table.Rows.Add(nameof(optional.LoaderFlags), optional.LoaderFlags.ToString("X"));
        table.Rows.Add(nameof(optional.NumberOfRvaAndSizes), optional.NumberOfRvaAndSizes.ToString("X"));
        
        return table;
    }

    private DataTable MakeCor20Header()
    {
        DataTable table = new()
        {
            Columns = { "Segment", "Value" },
            TableName = "CLR Part"
        };

        table.Rows.Add(nameof(model.CorHeader.SizeOfHead), model.CorHeader.SizeOfHead.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MajorRuntimeVersion), model.CorHeader.MajorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MinorRuntimeVersion), model.CorHeader.MinorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.MetaDataOffset), model.CorHeader.MetaDataOffset.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.LinkerFlags), model.CorHeader.LinkerFlags.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.EntryPointRva), model.CorHeader.EntryPointRva.ToString("X"));
        table.Rows.Add(nameof(model.CorHeader.EntryPointToken), model.CorHeader.EntryPointToken.ToString("X"));
        
        return table;
    }
    public void MakeSectionsTables()
    {
        DataTable sections = new()
        {
            TableName = "Sections Summary",
            Columns = 
            {
                "Name", 
                "VirtualAddress",
                "VirtualSize",
                "SizeOfRawData",
                "*RawData",
                "*Relocs",
                "*Line#",
                "#Relocs",
                "#LineNumbers",
                "Characteristics" 
            }
        };
        
        foreach (PeSection dump in model.Sections)
        {
            sections.Rows.Add(
                new String(dump.Name.Where(x => x != '\0').ToArray()),
                dump.VirtualAddress.ToString("X"),
                dump.VirtualSize.ToString("X"),
                dump.SizeOfRawData.ToString("X"),
                dump.PointerToRawData.ToString("X"),
                dump.PointerToRelocations.ToString("X"),
                dump.PointerToLinenumbers.ToString("X"),
                dump.NumberOfRelocations.ToString("X"),
                dump.NumberOfLinenumbers.ToString("X"),
                dump.Characteristics.ToString("X")
            );
        }

        Results.Add(sections);
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
            Columns = { "Name", "Value" }
        };

        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.Name), model.ExportTableModel.ExportDirectory.Name);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.MajorVersion), model.ExportTableModel.ExportDirectory.MajorVersion);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.MinorVersion), model.ExportTableModel.ExportDirectory.MinorVersion);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.Base), model.ExportTableModel.ExportDirectory.Base);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.AddressOfNames), model.ExportTableModel.ExportDirectory.AddressOfNames);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.AddressOfFunctions), model.ExportTableModel.ExportDirectory.AddressOfFunctions);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.NumberOfNames), model.ExportTableModel.ExportDirectory.NumberOfNames);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.NumberOfFunctions), model.ExportTableModel.ExportDirectory.NumberOfFunctions);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.AddressOfNameOrdinals), model.ExportTableModel.ExportDirectory.AddressOfNameOrdinals);
        exports.Rows.Add(nameof(model.ExportTableModel.ExportDirectory.TimeDateStamp), model.ExportTableModel.ExportDirectory.TimeDateStamp);

        DataTable functions = new()
        {
            TableName = "Exporting Functions",
            Columns = { "Name", "Ordinal", "Address" }
        };
        
        foreach (ExportFunction function in model.ExportTableModel.Functions)
        {
            functions.Rows.Add(
                function.Name,
                "@" + function.Ordinal,
                function.Address.ToString("X")
            );
        }

        Results.AddRange([exports, functions]);
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
                "From",
                "Name",
                "Ordinal",
                "Hint",
                "Address"
            }
        };

        foreach (ImportModule import in model.ImportTableModel.Modules)
        {
            foreach (ImportedFunction function in import.Functions)
            {
                imports.Rows.Add(
                    import.DllName,
                    function.Name,
                    "@" + function.Ordinal,
                    function.Hint.ToString("X"),
                    function.Address.ToString("X")
                );
            }
        }

        Results.Add(imports);
    }
}