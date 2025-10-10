using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Vb5ComDataVisualizer(VbComRegistration @struct) : AbstractStructVisualizer<VbComRegistration>(@struct)
{
    public override DataTable ToDataTable()
    {
        var dt = new DataTable()
        {
            Columns = {"Field:?", "Data:?"}
        };

        dt.Rows.Add("RegInfoOffset", $"0x{_struct.RegInfoOffset:x8}");
        dt.Rows.Add("ProjNameOffset", $"0x{_struct.ProjectNameOffset:x8}");
        dt.Rows.Add("HelpDirectoryOffset", $"0x{_struct.HelpDirectoryOffset:x8}");
        dt.Rows.Add("ProjDescriptionOffset", $"0x{_struct.ProjectDescriptionOffset:x8}");
        dt.Rows.Add("UUIDProjCLSID", $"0x{_struct.UuidProjectClsId:x16}");
        dt.Rows.Add("TypeLibLanguageId", $"0x{_struct.TypeLibraryLanguageId:x8}");
        dt.Rows.Add("Unknown", $"0x{_struct.Unknown:x4}");
        dt.Rows.Add("TypeLibMinor", _struct.TypeLibraryMinor);
        dt.Rows.Add("TypeLibMajor", _struct.TypeLibraryMajor);

        return dt;
    }

    public override string ToString()
    {
        return @"COM registration data is a complex table which has
extra data tables `ComRegistrationInfo` and Designer's metadata chains.
This is a second structure long pointer for what set in the `VB5Header`
or a `EXEPROJINFO` named struct.
 - ProjectName is a C-string
 - HelpDirectory is  a C-string
 - Description is a C-string";
    }

    public override Region ToRegion()
    {
        return new Region("## COM Registration Data", ToString(), ToDataTable());
    }
}