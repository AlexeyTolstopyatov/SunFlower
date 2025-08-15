using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;

namespace SunFlower.Windows.Services;
public static class MarkdownGenerator
{
    public static string Generate(IEnumerable<FlowerSeedResult> results)
    {
        StringBuilder md = new();
        md.AppendLine("# Sunflower Report");
        md.AppendLine($"**Generated at**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine();

        foreach (var result in results)
        {
            md.AppendLine(FormatFlowerSeedResult(result));
            md.AppendLine();
        }

        return md.ToString();
    }

    private static string FormatFlowerSeedResult(FlowerSeedResult result)
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
        if (table.Rows.Count == 0 || table.Columns.Count == 0)
            return string.Empty;
        
        string SafeToString(object value) => 
            Convert.IsDBNull(value) ? " " : value.ToString() ?? " ";
        
        var tableBuilder = new StringBuilder();
        
        var columns = table.Columns
            .Cast<DataColumn>()
            .ToArray();
        var rows = table.Rows
            .Cast<DataRow>()
            .ToArray();
    
        // row width processor
        var columnWidths = columns.Select(col => {
            var headerWidth = col.ColumnName.Length;
            var maxContentWidth = rows
                .Select(row => SafeToString(row[col]).Length)
                .DefaultIfEmpty(0)
                .Max();
            return Math.Max(headerWidth, maxContentWidth) + 2; // padding 2ch
        }).ToArray();

        // heading
        tableBuilder.AppendLine(FormatRow(columns.Select(c => c.ColumnName).ToArray()));
        
        var separator = string.Join("|", 
            columnWidths.Select(w => new string('-', w)));
        tableBuilder.AppendLine($"|--{separator}--|");
    
        // content
        foreach (var row in rows)
        {
            var rowData = columns
                .Select(col => SafeToString(row[col]))
                .ToArray();
            tableBuilder.AppendLine(FormatRow(rowData));
        }

        return tableBuilder.ToString();

        // delimiters
        string FormatRow(string[] cells)
        {
            var formatted = cells
                .Select((cell, i) => cell.PadRight(columnWidths[i]))
                .ToArray();
            return "| " + string.Join(" | ", formatted) + " |";
        }
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