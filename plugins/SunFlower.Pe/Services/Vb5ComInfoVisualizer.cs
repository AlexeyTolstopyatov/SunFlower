using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Vb5ComInfoVisualizer(VbComRegistrationInfo @struct) : AbstractStructVisualizer<VbComRegistrationInfo>(@struct)
{
    public override DataTable ToDataTable()
    {
        var dt = new DataTable()
        {
            Columns = { "Field", "Data" }
        };

        dt.Rows.Add("NextObjectOffset", $"0x{_struct.NextObjectOffset:X}");
        dt.Rows.Add("ObjectNameOffset", $"0x{_struct.ObjectNameOffset:X}");
        dt.Rows.Add("ObjectDescriptionOffset", $"0x{_struct.ObjectDescriptionOffset:X}");
        dt.Rows.Add("InstancingMode", $"0x{_struct.InstancingMode:X}");
        dt.Rows.Add("ObjectUUID", $"0x{_struct.UuidObject:X}");
        dt.Rows.Add("IsInterface", $"0x{_struct.IsInterface:X}");
        dt.Rows.Add("UUIDObjectInterfaceOffset", $"0x{_struct.UuidObjectInterfaceOffset:X}");
        dt.Rows.Add("UUIDEventsInterfaceOffset", $"0x{_struct.UuidEventsInterfaceOffset:X}");
        dt.Rows.Add("HasEvents?", $"0x{_struct.HasEvents:X}");
        dt.Rows.Add("MISCStatus", $"0x{_struct.MiscStatus:X}");
        dt.Rows.Add("ClassType", $"0x{_struct.ClassType:X}");
        dt.Rows.Add("ToolBoxBitmap32Id", $"0x{_struct.ToolBoxBitmap32:X}");
        dt.Rows.Add("DefaultIcon", $"0x{_struct.DefaultIcon:X}");
        dt.Rows.Add("IsDesigner", $"0x{_struct.IsDesigner:X}");
        dt.Rows.Add("DesignerDataOffset", $"{_struct.DesignerDataOffset:X}");
        
        return dt;
    }

    public override string ToString()
    {
        return @"COM registration info
follows by the COM registration data table and has
details of current _?ActiveX?_ object.";
    }

    public override Region ToRegion()
    {
        return new Region("### COM Registration Information", ToString(), ToDataTable());
    }
}