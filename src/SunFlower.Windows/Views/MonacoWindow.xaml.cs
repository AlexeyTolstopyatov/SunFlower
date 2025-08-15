using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.ViewModels;

namespace SunFlower.Windows.Views;

public partial class MonacoWindow : HandyControl.Controls.Window
{
    private readonly MonacoWindowViewModel _viewModel;
    private readonly MonacoEditorManager _monacoEditorManager;
    public MonacoWindow(List<IFlowerSeed> seeds)
    {
        InitializeComponent();
        
        _monacoEditorManager = new MonacoEditorManager(View2);
        _viewModel = new MonacoWindowViewModel(seeds);
        DataContext = _viewModel;
        
        Loaded += async (s, e) => 
        {
            //await _monacoController.SetThemeAsync();
            
            // prepare results
            List<FlowerSeedResult> results = [];
            foreach (var seed in seeds)
            {
                results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
                {
                    BoxedResult = new List<string>(){seed.Seed}
                });
                foreach (var result in seed.Status.Results)
                {
                    results.Add(result);
                }
            }
            
            await _monacoEditorManager.UpdateMarkdownReportAsync(results);
        };
    }
}