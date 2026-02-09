using System.Collections.ObjectModel;
using System.Windows.Input;
using HandyControl.Controls;
using Microsoft.Xaml.Behaviors.Core;
using SunFlower.Abstractions;
using SunFlower.Windows.Services;
using SunFlower.Windows.Views;

namespace SunFlower.Windows.ViewModels;

public class PropertiesViewModel : NotifyPropertyChanged
{
    private ObservableCollection<IFlowerSeed> _plugins;
    private WindowManager _windowManager;
    private string _path;

    public PropertiesViewModel(ObservableCollection<IFlowerSeed> plugins, string path)
    {
        _plugins = plugins;
        _path = path;
        _windowManager = new();
        CallEditorCommand = new ActionCommand(CallEditor);
    }

    public PropertiesViewModel(): this([], "nothing to see") { }

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
    public ICommand CallEditorCommand { get; init; }

    private void CallEditor()
    {
        try
        {
            _windowManager.ShowUnmanaged(new MonacoWindow(
                    Plugins
                        .Where(s => s.Status.IsEnabled)
                        .ToList()),
                title: Path,
                isDialog: false);
        }
        catch (Exception e)
        {
            Growl.ErrorGlobal(e.Message);
        }
    }
}