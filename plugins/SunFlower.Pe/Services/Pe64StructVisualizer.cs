using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Pe64StructVisualizer(PeOptionalHeader @struct) : AbstractStructVisualizer<PeOptionalHeader>(@struct)
{
    private readonly string _content = "### Optional `PE32+` (64-bit) Header";
    public override DataTable ToDataTable()
    {
        DataTable table = new()
        {
            TableName = "Optional Part (64-bit fields)"
        };
        table.Columns.AddRange([new DataColumn("Segment"), new DataColumn("Value")]);

        table.Rows.Add(nameof(_struct.Magic), "0x" + _struct.Magic.ToString("X"));
        table.Rows.Add(nameof(_struct.MajorLinkerVersion), "0x" + _struct.MajorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MinorLinkerVersion), "0x" + _struct.MinorLinkerVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfCode), "0x" + _struct.SizeOfCode.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfInitializedData), "0x" + _struct.SizeOfInitializedData.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfUninitializedData), "0x" + _struct.SizeOfUninitializedData.ToString("X"));
        table.Rows.Add(nameof(_struct.AddressOfEntryPoint), "0x" + _struct.AddressOfEntryPoint.ToString("X"));
        table.Rows.Add(nameof(_struct.BaseOfCode), "0x" + _struct.BaseOfCode.ToString("X"));
        table.Rows.Add(nameof(_struct.BaseOfData), "0x" + _struct.BaseOfData.ToString("X"));
        table.Rows.Add(nameof(_struct.ImageBase), "0x" + _struct.ImageBase.ToString("X"));
        table.Rows.Add(nameof(_struct.SectionAlignment), "0x" + _struct.SectionAlignment.ToString("X"));
        table.Rows.Add(nameof(_struct.FileAlignment), "0x" + _struct.FileAlignment.ToString("X"));
        table.Rows.Add(nameof(_struct.MajorOperatingSystemVersion), "0x" + _struct.MajorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MinorOperatingSystemVersion), "0x" + _struct.MinorOperatingSystemVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MajorSubsystemVersion), "0x" + _struct.MajorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MinorSubsystemVersion), "0x" + _struct.MinorSubsystemVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MajorImageVersion), "0x" + _struct.MajorImageVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MinorImageVersion), "0x" + _struct.MinorImageVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.Win32VersionValue), "0x" + _struct.Win32VersionValue.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfImage), "0x" + _struct.SizeOfImage.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfHeaders), "0x" + _struct.SizeOfHeaders.ToString("X"));
        table.Rows.Add(nameof(_struct.CheckSum), "0x" + _struct.CheckSum.ToString("X"));
        table.Rows.Add(nameof(_struct.Subsystem), "0x" + _struct.Subsystem.ToString("X"));
        table.Rows.Add(nameof(_struct.DllCharacteristics), "0x" + _struct.DllCharacteristics.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfStackReserve), "0x" + _struct.SizeOfStackReserve.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfStackCommit), "0x" + _struct.SizeOfStackCommit.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfHeapReserve), "0x" + _struct.SizeOfHeapReserve.ToString("X"));
        table.Rows.Add(nameof(_struct.SizeOfHeapCommit), "0x" + _struct.SizeOfHeapCommit.ToString("X"));
        table.Rows.Add(nameof(_struct.LoaderFlags), "0x" + _struct.LoaderFlags.ToString("X"));
        table.Rows.Add(nameof(_struct.NumberOfRvaAndSizes), "0x" + _struct.NumberOfRvaAndSizes.ToString("X"));
        
        return table;
    }

    public override string ToString()
    {
        return new FlowerDescriptor()
            .Line("This header names \"optional\" because isn't necessary for all PE linked files.")
            .Line("Depends on ").Inline("wMagic")
            .Line("field defines maximum word size for HEAP and STACK characteristics.")
            .Line("Some data don't check by loader and may be empty or incorrect.")
            .ToString();
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}