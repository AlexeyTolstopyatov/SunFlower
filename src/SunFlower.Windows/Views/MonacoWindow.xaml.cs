using HandyControl.Themes;
using Microsoft.Web.WebView2.WinForms;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.ViewModels;

namespace SunFlower.Windows.Views;

public partial class MonacoWindow : HandyControl.Controls.Window
{
    private readonly MonacoWindowViewModel _viewModel;
    private readonly MonacoController _monacoController;
    public MonacoWindow(List<IFlowerSeed> seeds)
    {
        InitializeComponent();
        
        _monacoController = new MonacoController(View2);
        _viewModel = new MonacoWindowViewModel(_monacoController, seeds);
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
            
            await _monacoController.UpdateMarkdownReportAsync(results);
        };
#if DEBUG
        View2.CoreWebView2InitializationCompleted += (s, e) => 
        {
            if (e.IsSuccess)
            {
                View2.CoreWebView2.OpenDevToolsWindow();
            }
        };
#endif
    }
}