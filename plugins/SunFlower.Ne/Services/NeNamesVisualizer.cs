using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Services;

public class NeNamesVisualizer(List<Name> @struct, bool resident) : AbstractStructVisualizer<List<Name>>(@struct)
{
    public override DataTable ToDataTable()
    {
        DataTable table = new()
        {
            Columns = { "Ordinal", "Length", "Name" }
        };
        foreach (var neExportDump in _struct)
        {
            table.Rows.Add(
                "@" + neExportDump.Ordinal,
                neExportDump.Count,
                FlowerReport.SafeString(neExportDump.String)
            );
        }

        return table;
    }

    public override string ToString()
    {
        var res = resident 
            ? @"Resident Names Table represents ordinals and Pascal-Strings of
exporting functions which can be used by module while
instance of this module is loaded." 
            : @"Non-resident names table represents ordinals and Pascal-Strings
of exporting procedures or functions which unused by current module
while it loaded in memory";
                
        return res;
    }

    public override Region ToRegion()
    {
        var heading = resident switch
        {
            true => "### Resident Names Table",
            false => "### Non-Resident Names Table"
        };
        
        return new Region(heading, ToString(), ToDataTable());
    }
}