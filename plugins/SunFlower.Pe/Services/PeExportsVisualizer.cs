using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeExportsVisualizer(List<ExportFunction> @struct) : AbstractStructVisualizer<List<ExportFunction>>(@struct)
{
    private readonly string _content = "### Exports";
    private readonly List<ExportFunction> _struct1 = @struct;

    public override string ToString()
    {
        return
            "The export name table contains the actual string data that was pointed to by the export name pointer table. " +
            "The strings in this table are public names that other images can use to import the symbols. " +
            "These public export names are not necessarily the same as the private symbol names that " +
            "the symbols have in their own image file and source code, although they can be.\n\n" +
            "Every exported symbol has an ordinal value, which is just the index into the export address table. " +
            "Use of export names, however, is optional. Some, all, or none of the exported symbols can have export names. " +
            "For exported symbols that do have export names, corresponding entries in the export name pointer table and export ordinal table " +
            "work together to associate each name with an ordinal.";
    }

    public override DataTable ToDataTable()
    {
        DataTable functions = new()
        {
            TableName = "Exporting Functions",
            Columns = { "Name:s", "Ordinal:2", "Address:8" }
        };
        
        foreach (var function in _struct1)
        {
            functions.Rows.Add(
                function.Name,
                "@" + function.Ordinal,
                function.Address.ToString("X")
            );
        }

        return functions;
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}