using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;


namespace SunFlower.Pe.Services;

public class Pe32StructVisualizer(PeOptionalHeader32 @struct) : AbstractStructVisualizer<PeOptionalHeader32>(@struct)
{
    private readonly string _content = "### Optional `PE32` Header";
    
    public override DataTable ToDataTable()
    {
        DataTable table = new()
        {
            TableName = "Optional Part (32-bit fields)"
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

    public override Region ToRegion()
    {
        var content = new StringBuilder();
        content.AppendLine(
            "This header named \"optional\" because not for all PE linked objects this header must been");
        content.AppendLine("Depends on `Magic` field defines maximum bounds for heap and stack characteristics");
        content.AppendLine("Some data in this header not checks by loader in the runtime -> can be changed manually");
        
        return new Region(_content, content.ToString(), ToDataTable());
    }
}