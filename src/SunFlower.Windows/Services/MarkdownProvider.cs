using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Kernel.Writers;

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
            FlowerSeedEntryType.DataTables => FlowerMarkdownWriter.FormatTables((IEnumerable<DataTable>)result.BoxedResult),
            FlowerSeedEntryType.Regions => FlowerMarkdownWriter.FormatRegions((IEnumerable<Region>)result.BoxedResult),
            _ => $"Result type [{result.Type}] not supported yet"
        };
    }
}