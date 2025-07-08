using System.Collections.ObjectModel;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;
using SunFlower.Windows.Views;

namespace SunFlower.Windows.ViewModels;

public class MonacoWindowViewModel : NotifyPropertyChanged
{
    private readonly MonacoController _controller;
    public MonacoWindowViewModel(MonacoController controller, List<IFlowerSeed> seeds)
    {
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
        _controller = controller;
        _results = new(results);
    }
    
    private ObservableCollection<FlowerSeedResult> _results = [];

    public ObservableCollection<FlowerSeedResult> Results
    {
        get => _results;
        set
        {
            _results = value;
            OnPropertyChanged();
            UpdateReport();
        }
    }

    public void AddResult(FlowerSeedResult result)
    {
        _results.Add(result);
        UpdateReport();
    }

    private async void UpdateReport()
    {
        await _controller.UpdateMarkdownReportAsync(Results);
    }
}
