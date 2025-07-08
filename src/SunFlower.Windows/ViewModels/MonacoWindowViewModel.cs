using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors.Core;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class MonacoWindowViewModel : NotifyPropertyChanged
{
    private readonly List<FlowerSeedResult> _results;

    public MonacoWindowViewModel()
    {
        _results = [];
        _saveResultsCommand = new ActionCommand(SaveResults);
    }
    public MonacoWindowViewModel(List<IFlowerSeed> seeds)
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
        _results = results;
        _saveResultsCommand = new ActionCommand(SaveResults);
    }

    public ICommand SaveResultsCommand
    {
        get => _saveResultsCommand;
        set => SetField(ref _saveResultsCommand, value);
    }
    private ICommand _saveResultsCommand;

    private void SaveResults()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "Markdown Document (*.md)|*.md|All types (*.*)|*.*",
        };
        
        dialog.ShowDialog();
        
        File.WriteAllText(dialog.FileName, MarkdownGenerator.GenerateReport(_results));
    }
    
}
