using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class PeDirectoriesVisualizer(PeDirectory[] @struct) : AbstractStructVisualizer<PeDirectory[]>(@struct)
{
    private readonly string _content = "## Directories";
    private static readonly string[] Names =
    {
        "`EXPORT`", 
        "`IMPORT`",
        "`RESOURCE`",
        "`EXCEPTION`",
        "`SECURITY`",
        "`BASE_RELOC`",
        "`DEBUG`",
        "`ARCHITECTURE`",
        "`GLOBAL_PTR`",
        "`TLS`",
        "`LOAD_CONFIG`",
        "`BOUND_IMPORT`",
        "`IAT`",
        "`DELAY_IMPORT`",
        "`COM_DESCRIPTOR`",
        "`RESERVED!`"
    };
    private static readonly string[] OtherNames =
    {
        "", // exports
        "`STATIC_IMPORTS`",
        "", // rsrc
        "`SEH`",
        "`CRT` (or Certificates Table)", // security
        "", // base relocations
        "", // debug
        "`COPYRIGHT`",
        "`MIPS_GLOBALPTR`", // global ptr
        "", // TLS,
        "`CONFIGURATION`",
        "", // bound import
        "", // IAT
        "",
        "`NET_METADATA`",
        ""
    };

    private readonly PeDirectory[] _struct = @struct;
    public override string ToString()
    {
        var content = new StringBuilder();
        content.AppendLine("Logical parts of executable image, which data may contain in other separated sections");
        return content.ToString();
    }

    public override DataTable ToDataTable()
    {
        var d = new DataTable
        {
            Columns = { "#:4", "Naming:s", "OtherNaming:s", "RVA:4", "Size:4" }
        };

        for (var i = 0; i < _struct.Length; ++i)
        {
            d.Rows.Add((i + 1), 
                Names[i], 
                OtherNames[i],
                "0x" + _struct[i].VirtualAddress.ToString("X4"),
                "0x" + _struct[i].Size.ToString("X4"));
        }

        return d;
    }
    
    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}