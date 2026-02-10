using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Writers;

namespace SunFlower.Windows.Services;
public static class MarkdownProvider
{
    public static string Provide(IEnumerable<FlowerSeedResult> results)
    {
        StringBuilder md = new();
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
            FlowerSeedEntryType.Strings => FlowerMarkdownWriter.FormatStrings(result.BoxedResult),
            FlowerSeedEntryType.DataTables => FormatDataTables((List<DataTable>)result.BoxedResult),
            FlowerSeedEntryType.Regions => FormatRegions((List<Region>)result.BoxedResult),
            _ => $"Result type [{result.Type}] not supported yet"
        };
    }
    
    private static string FormatDataTables(List<DataTable> dataTables)
    {
        StringBuilder tablesString = new();
        foreach (var dataTable in dataTables)
        {
            tablesString.AppendLine($"### {dataTable.TableName}");
            tablesString.AppendLine(FlowerMarkdownWriter.FormatTable(dataTable));
            tablesString.AppendLine();
        }

        return tablesString.ToString();
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
        builder.AppendLine(FlowerMarkdownWriter.FormatTable(reg.Table));
        
        return builder.ToString();
    }
}