using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Cor20StructVisualizer(Cor20Header @struct) : AbstractStructVisualizer<Cor20Header>(@struct)
{
    private readonly string _content = "### CLR Header";
    private readonly Cor20Header _struct = @struct;
    
    public override DataTable ToDataTable()
    {
        DataTable table = new()
        {
            Columns = { "Segment", "Value" },
            TableName = "CLR Part"
        };

        table.Rows.Add(nameof(_struct.SizeOfHead), "0x" + _struct.SizeOfHead.ToString("X"));
        table.Rows.Add(nameof(_struct.MajorRuntimeVersion), "0x" + _struct.MajorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MinorRuntimeVersion), "0x" + _struct.MinorRuntimeVersion.ToString("X"));
        table.Rows.Add(nameof(_struct.MetaDataOffset), "0x" + _struct.MetaDataOffset.ToString("X"));
        table.Rows.Add(nameof(_struct.LinkerFlags), "0x" + _struct.LinkerFlags.ToString("X"));
        table.Rows.Add(nameof(_struct.EntryPointRva), "0x" + _struct.EntryPointRva.ToString("X"));
        table.Rows.Add(nameof(_struct.EntryPointToken), "0x" + _struct.EntryPointToken.ToString("X"));
        
        return table;
    }

    public override Region ToRegion()
    {
        var content = new StringBuilder();
        content.AppendLine("This is a .NET assembly common information section. ");
        content.AppendLine(FlowerReport.ForColumn("SizeOfHead", typeof(int)) + "always equals 0x48 for the correct build .NET assemblies");
        content.AppendLine("This header tells that build has Microsoft `IL` instructions and all project metadata");
        
        return new Region(_content, content.ToString(), ToDataTable());
    }
}