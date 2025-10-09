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
        SaveResultsCommand = new ActionCommand(SaveResults);
    }
    public MonacoWindowViewModel(List<IFlowerSeed> seeds)
    {
        List<FlowerSeedResult> results = [];
        foreach (var seed in seeds)
        {
            results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = new List<string> {seed.Seed}
            });
            
            results.AddRange(seed.Status.Results);
        }
        _results = results;
        SaveResultsCommand = new ActionCommand(SaveResults);
    }

    public ICommand SaveResultsCommand { get; }

    private void SaveResults()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "Markdown Document (*.md)|*.md|All types (*.*)|*.*",
            ShowHiddenItems = true
        };

        dialog.ShowDialog();
        
        if (string.IsNullOrEmpty(dialog.FileName)) 
            return;
        
        File.WriteAllText(dialog.FileName, MarkdownGenerator.Generate(_results));
    }
    
}
