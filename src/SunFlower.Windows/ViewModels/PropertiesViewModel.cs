using System.Collections.ObjectModel;
using SunFlower.Abstractions;

namespace SunFlower.Windows.ViewModels;

public class PropertiesViewModel : NotifyPropertyChanged
{
    private ObservableCollection<IFlowerSeed> _plugins;
    private string _path;

    public PropertiesViewModel(ObservableCollection<IFlowerSeed> plugins, string path)
    {
        _plugins = plugins;
        _path = path;
    }

    public PropertiesViewModel() : this([], "nothing to see") { }

    public ObservableCollection<IFlowerSeed> Plugins
    {
        get => _plugins;
        set => SetField(ref _plugins, value);
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }
}