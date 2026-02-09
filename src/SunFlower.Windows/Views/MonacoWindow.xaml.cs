using System.Collections.ObjectModel;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.ViewModels;

namespace SunFlower.Windows.Views;

public partial class MonacoWindow : HandyControl.Controls.Window
{
    public MonacoWindow(ObservableCollection<IFlowerSeed> seeds) : this(seeds.ToList()) { }

    public MonacoWindow(List<IFlowerSeed> seeds)
    {
        InitializeComponent();
        
        var monacoEditorManager = new MonacoEditorManager(View2);
        var viewModel = new MonacoViewModel(seeds);
        DataContext = viewModel;
        
        Loaded += async (s, e) => 
        {
            //await _monacoController.SetThemeAsync();
            
            // prepare results
            List<FlowerSeedResult> results = [];
            foreach (var seed in seeds)
            {
                results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
                {
                    BoxedResult = new List<string> {seed.Seed}
                });
                if (seed.Status.LastError is not null)
                {
                    // errors occured -> write them
                    results.Add(new FlowerSeedResult(
                        FlowerSeedEntryType.Strings, 
                        "```\n" + seed.Status.LastError + "\n```"));
                }
                foreach (var result in seed.Status.Results)
                {
                    results.Add(result);
                }
            }
            
            await monacoEditorManager.UpdateMarkdownReportAsync(results);
        };
    }
}