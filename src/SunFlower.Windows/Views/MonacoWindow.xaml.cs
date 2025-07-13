using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.ViewModels;

namespace SunFlower.Windows.Views;

public partial class MonacoWindow : HandyControl.Controls.Window
{
    private readonly MonacoWindowViewModel _viewModel;
    private readonly MonacoEditorService _monacoEditorService;
    public MonacoWindow(List<IFlowerSeed> seeds)
    {
        InitializeComponent();
        
        _monacoEditorService = new MonacoEditorService(View2);
        _viewModel = new MonacoWindowViewModel(seeds);
        DataContext = _viewModel;
        
        Loaded += async (s, e) => 
        {
            //await _monacoController.SetThemeAsync();
            
            // prepare results
            List<FlowerSeedResult> results = [];
            foreach (IFlowerSeed seed in seeds)
            {
                results.Add(new FlowerSeedResult()
                {
                    Type = FlowerSeedEntryType.Text,
                    BoxedResult = new List<string>(){seed.Seed}
                });
                foreach (FlowerSeedResult result in seed.Status.Results)
                {
                    results.Add(result);
                }
            }
            
            await _monacoEditorService.UpdateMarkdownReportAsync(results);
        };
    }
}