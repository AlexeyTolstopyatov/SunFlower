using System.Windows.Controls;
using HandyControl.Controls;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.ViewModels.Tabs;

namespace SunFlower.Windows.Views;
public partial class MonacoControl : UserControl
{
    private readonly MonacoManager _manager;
        
    public MonacoControl()
    {
        InitializeComponent();
        _manager = new MonacoManager(View2);
        
        Loaded += async (_, _) => await InitializeMonacoAsync();
    }
        
    private async Task InitializeMonacoAsync()
    {
        if (DataContext is not MonacoTab tab)
        {
            Growl.ErrorGlobal($"Unexpected DataContext: {DataContext}");
            return;
        }
        
        List<FlowerSeedResult> results = [];
        Console.WriteLine($"Opening [{tab.Id}]");
        
        foreach (var seed in tab.Plugins)
        {
            results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = new List<string> { seed.Seed }
            });
            if (seed.Status.LastError is not null)
            {
                // errors occured -> write them
                results.Add(new FlowerSeedResult(
                    FlowerSeedEntryType.Strings,
                    "```\n" + seed.Status.LastError + "\n```"));
            }

            results.AddRange(seed.Status.Results);
        }

        await UpdateContent(results);
    }
        
    public async Task UpdateContent(List<FlowerSeedResult> results)
    {
        await _manager.UpdateMarkdownReportAsync(results);
    }
}