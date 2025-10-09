using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeImportsVisualizer(PeImportTableModel @struct) : AbstractStructVisualizer<PeImportTableModel>(@struct)
{
    private readonly string _content = "### Imports";
    
    public override DataTable ToDataTable()
    {
        DataTable imports = new()
        {
            TableName = "Import Names Summary",
            Columns =
            {
                "Module:s",
                "Procedure:s",
                "Ordinal:2",
                "Hint:2",
                "Address:8"
            }
        };

        foreach (var import in _struct.Modules)
        {
            foreach (var function in import.Functions)
            {
                imports.Rows.Add(
                    import.DllName,
                    function.Name,
                    "@" + function.Ordinal,
                    "0x" + function.Hint.ToString("X4"),
                    "0x" + function.Address.ToString("X16")
                );
            }
        }

        return imports;
    }

    public override string ToString()
    {
        return "Usually the import information begins with the import directory table, " +
               "which describes the remainder of the import information. " +
               "The import directory table contains address information that is used to resolve fixup references " +
               "to the entry points within a DLL image. The import directory table consists of an array of import directory entries, " +
               "one entry for each DLL to which the image refers. " +
               "The last directory entry is empty (filled with null values), " +
               "which indicates the end of the directory table.";
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}