using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class PeExportDirectoryVisualizer(PeExportTableModel @struct) : AbstractStructVisualizer<PeExportTableModel>(@struct)
{
    private readonly string _content = "### Exports Header";
    private readonly PeExportTableModel _struct1 = @struct;

    public override string ToString()
    {
        return "The export symbol information begins with the export directory table, " +
               "which describes the remainder of the export symbol information. " +
               "The export directory table contains address information that is used to resolve imports to the " +
               "entry points within this image.";
    }

    public override DataTable ToDataTable()
    {
        DataTable exports = new()
        {
            TableName = "Export Directory",
            Columns = { "Name:s", "Value:?" }
        };

        exports.Rows.Add("Name", _struct1.ExportDirectory.Name);
        exports.Rows.Add("MajorVersion", _struct1.ExportDirectory.MajorVersion);
        exports.Rows.Add("MinorVersion", _struct1.ExportDirectory.MinorVersion);
        exports.Rows.Add("Base", "0x" + _struct1.ExportDirectory.Base.ToString("X"));
        exports.Rows.Add("NamesAddress", "0x" + _struct1.ExportDirectory.AddressOfNames.ToString("X"));
        exports.Rows.Add("ProceduresAddress", "0x" + _struct1.ExportDirectory.AddressOfFunctions.ToString("X"));
        exports.Rows.Add("Names#", _struct1.ExportDirectory.NumberOfNames);
        exports.Rows.Add("Procedures#", _struct1.ExportDirectory.NumberOfFunctions);
        exports.Rows.Add("NameOrdinalsAddress", "0x" + _struct1.ExportDirectory.AddressOfNameOrdinals.ToString("X"));
        exports.Rows.Add("TimeStamp", "0x" + _struct1.ExportDirectory.TimeDateStamp);

        return exports;
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}