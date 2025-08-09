using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;

namespace SunFlower.Windows.Services;
public static class MarkdownGenerator
{
    public static string GenerateReport(IEnumerable<FlowerSeedResult> results)
    {
        StringBuilder md = new();
        md.AppendLine("# Sunflower Report");
        md.AppendLine($"**Generated at**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine();

        foreach (var result in results)
        {
            md.AppendLine(FormatResult(result));
            md.AppendLine();
        }

        return md.ToString();
    }

    private static string FormatResult(FlowerSeedResult result)
    {
        return result.Type switch
        {
            FlowerSeedEntryType.Strings => FormatStrings(result.BoxedResult),
            FlowerSeedEntryType.DataTables => FormatDataTables((List<DataTable>)result.BoxedResult),
            FlowerSeedEntryType.Regions => FormatRegions((List<Region>)result.BoxedResult),
            _ => "Unsupported result type"
        };
    }

    private static string FormatStrings(object lines)
    {
        try
        {
            switch (lines)
            {
                case string line:
                    return line;
                case string[] linesArray:
                    return "\n" + string.Join("\n", linesArray) + "\n";
                case List<string> linesList:
                    return "\n" + string.Join("\n", linesList) + "\n";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return $"[Unknown Type `{lines}`]";
    }

    private static string FormatDataTables(List<DataTable> dataTables)
    {
        StringBuilder tablesString = new();
        foreach (var dataTable in dataTables)
        {
            tablesString.AppendLine($"### {dataTable.TableName}");
            tablesString.AppendLine(FormatDataTable(dataTable));
            tablesString.AppendLine();
        }

        return tablesString.ToString();
    }
    
    private static string FormatDataTable(DataTable table)
    {
        StringBuilder sb = new();
        
        sb.Append("| ");
        foreach (DataColumn col in table.Columns)
        {
            sb.Append($"{col.ColumnName} | ");
        }
        sb.AppendLine();
        
        sb.Append("|");
        for (var i = 0; i < table.Columns.Count; i++)
        {
            sb.Append("---|");
        }
        sb.AppendLine();
        
        foreach (DataRow row in table.Rows)
        {
            sb.Append("| ");
            for (var i = 0; i < table.Columns.Count; i++)
            {
                sb.Append($"{row[i]} | ");
            }
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    private static string FormatRegions(List<Region> regions)
    {
        StringBuilder builder = new();
        //var iter = 1;
        foreach (var region in regions)
        {
            builder.AppendLine(FormatRegion(region));
            builder.AppendLine();
        }

        return builder.ToString();
    }
    private static string FormatRegion(Region reg /*, int iter*/)
    {
        StringBuilder builder = new();
        
        builder.AppendLine(reg.Head);
        builder.AppendLine();

        builder.AppendLine(reg.Content);
        builder.AppendLine(FormatDataTable(reg.Table));
        
        return builder.ToString();
    }
}