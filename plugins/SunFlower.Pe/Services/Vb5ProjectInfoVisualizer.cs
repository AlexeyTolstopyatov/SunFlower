using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Vb5ProjectInfoVisualizer(Vb5ProjectInfo @struct) : AbstractStructVisualizer<Vb5ProjectInfo>(@struct)
{
    public override DataTable ToDataTable()
    {
        var dt = new DataTable
        {
            Columns = { "Field:?", "Data:?" }
        };

        // unicode strings here.
        var primitivePathUnicode = Encoding.Unicode.GetString(_struct.PrimitivePath);
        var projectPathUnicode = Encoding.Unicode.GetString(_struct.ProjectPath);
        
        dt.Rows.Add("dwVersion", $"0x{_struct.Version:X8}");
        dt.Rows.Add("lpObjectTable", $"0x{_struct.ObjectTablePointer:X8}");
        dt.Rows.Add("dwNULL", $"0x{_struct.Null:X8}");
        dt.Rows.Add("lpCodeStart", $"0x{_struct.CodeStartsPointer:X8}");
        dt.Rows.Add("lpCodeEnd", $"0x{_struct.CodeEndsPointer:X8}");
        dt.Rows.Add("dwDataSize", $"0x{_struct.DataSize:X8}");
        dt.Rows.Add("lpThreadSpace", $"0x{_struct.ThreadSpacePointer:X8}");
        dt.Rows.Add("lpVBASEH", $"0x{_struct.VbaSEHPointer:X8}");
        dt.Rows.Add("lpNativeCode", $"0x{_struct.NativeCodePointer:X8}");
        dt.Rows.Add("wsPrimitivePath", FlowerReport.SafeString(primitivePathUnicode));
        dt.Rows.Add("wsProjectPath", FlowerReport.SafeString(projectPathUnicode));
        dt.Rows.Add("lpExternalCtlsTable", $"0x{_struct.ExternalTablePointer:X8}");
        dt.Rows.Add("dwExternalCtlsCount", $"0x{_struct.ExternalTableCount:X8}");

        return dt;
    }

    public override string ToString()
    {
        return @"
> [!INFO]
> Filled at `vba6.dll!0x0FB11783`
The `ProjectInfo` structure is a structure
always filled by the Visual Basic 5.0+ Compiler. 
 - `dwVersion` set as 0x1F4 (0d500)
 - `lpExternalTable` points to the `ExternalApiDescriptor`s array
 - `lpCodeStart` may has `0xE9E9E9E9`
 - `lpCodeEnd` may has `0x9E9E9E9E`
 - If Native code pointer set to 0 - this is a P-Code compiled module
 - `ExternalApiDescriptor` is a call of procedure from another module. (e.g. `User32.DLL`)";
    }

    public override Region ToRegion()
    {
        return new Region("### VB 5.0+ Project Information", ToString(), ToDataTable());
    }
}