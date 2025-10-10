using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace SunFlower.Pe.Services;

public class Vb5HeaderDetailsManager(Vb5ProjectTablesManager @struct) : AbstractStructVisualizer<Vb5ProjectTablesManager>(@struct)
{
    public override DataTable ToDataTable()
    {
        var dt = new DataTable()
        {
            Columns = { "Name", "Value" }
        };

        dt.Rows.Add("ProjectNameOffset", FlowerReport.SafeString(_struct.ProjectName));
        dt.Rows.Add("ProjectEXENameOffset", FlowerReport.SafeString(_struct.ProjectExeName));
        dt.Rows.Add("ProjectDesctiptionOffset", FlowerReport.SafeString(_struct.ProjectDescription));

        return dt;
    }

    public override string ToString()
    {
        return @"
Based on a Visual Basic 5.0+ Runtime header some details
was extracted

The first signature fills by compiler and has `VB5!` ASCII string
but virtual machine components _never checks those bytes_ and
in the fact this field may contain anything

Table in this section contains all strings read by those offsets. 
";
    }

    public override Region ToRegion()
    {
        return new Region("### Visual Basic 5.0+ Header strings", ToString(), ToDataTable());
    }
}