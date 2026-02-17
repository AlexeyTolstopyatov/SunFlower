using System.Collections.ObjectModel;
using SunFlower.Windows.ViewModels.Tabs;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using SunFlower.Abstractions;
using SunFlower.Kernel.Services;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class WorkspaceViewModel : NotifyPropertyChanged
{
    private readonly FlowerSeedManager _manager;
    
    private FileModel _fileModel;
    public FileModel FileModel
    {
        get => _fileModel;
        set => SetField(ref _fileModel, value);
    }

    private ObservableCollection<IFlowerSeed> _availablePlugins;
    public ObservableCollection<IFlowerSeed> AvailablePlugins
    {
        get => _availablePlugins;
        set => SetField(ref _availablePlugins, value);
    }

    private ObservableCollection<WorkspaceTab> _tabs;
    public ObservableCollection<WorkspaceTab> Tabs
    {
        get => _tabs;
        set => SetField(ref _tabs, value);
    }

    private WorkspaceTab _selectedTab;
    public WorkspaceTab SelectedTab
    {
        get => _selectedTab;
        set => SetField(ref _selectedTab, value);
    }

    public ICommand SwitchToHexEditorCommand { get; }
    public ICommand SwitchToHexViewerCommand { get; }
    public ICommand SwitchToMonacoCommand { get; }
    public ICommand OpenPluginTabCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand SwitchToStatusCommand { get; }
    public ICommand SwitchOnePluginReportCommand { get; }
    public ICommand OpenNotEmptyTabs { get; }

    public WorkspaceViewModel()
    {
        _tabs = [];
        _availablePlugins = [];
        _fileModel = new FileModel();
        _selectedTab = new StatusTab();
        
        CloseTabCommand = new ActionCommand(CloseTab);
        SwitchToHexEditorCommand = new ActionCommand(OpenHexEditor);
        SwitchToHexViewerCommand = new ActionCommand(OpenHexViewer);
        SwitchToMonacoCommand = new ActionCommand(OpenMonacoReport);
        SwitchToStatusCommand = new ActionCommand(OpenStatusControl);
        SwitchOnePluginReportCommand = new DelegateCommand<IFlowerSeed>(OnePluginMonacoReport);
        OpenNotEmptyTabs = new ActionCommand(OpenNotEmptyReflectionTabs);
        OpenPluginTabCommand = new DelegateCommand<IFlowerSeed>(OpenReflectionTab);
        
        _manager = FlowerSeedManager
            .CreateInstance()
            .LoadAllFlowerSeeds();
    }

    public WorkspaceViewModel(FileModel fileModel) : this()
    {
        FileModel = fileModel;
        _manager
            .UpdateAllInvokedFlowerSeeds(fileModel.FullName);

        foreach (var seed in _manager.Seeds)
            AvailablePlugins.Add(seed);
    }
    private void OpenStatusControl()
    {
        var statusTab = new StatusTab
        {
            Title = "Sunflower Review",
            Icon = "📝",
            CanClose = true,
            Context = new StatusControlViewModel
            {
                FileModel = _fileModel
            }
        };

        OpenOrActivateTab(statusTab);
    }
    private void OpenHexEditor()
    {
        var hexTab = new HexEditorTab
        {
            Title = "Hexa Editor",
            Icon = "🔍",
            Context = new HexViewModel(_fileModel.FullName),
            CanClose = true
        };

        OpenOrActivateTab(hexTab);
    }

    private void OpenHexViewer()
    {
        var hexTab = new HexViewTab
        {
            Title = "Hex View",
            Icon = "🔍",
            Context = new(),
            CanClose = true
        };
        hexTab.Context.InitializeStream(_fileModel.FullName);
        OpenOrActivateTab(hexTab);
    }

    private void OnePluginMonacoReport(IFlowerSeed s)
    {
        var monacoTab = new MonacoTab
        {
            Title = $"{s.Seed} - Report",
            Icon = "📝",
            Plugins = [s],
            CanClose = true
        };

        OpenOrActivateTab(monacoTab);
    }
    
    private void OpenMonacoReport()
    {
        var monacoTab = new MonacoTab
        {
            Title = "Analysis Report",
            Icon = "📝",
            Plugins = AvailablePlugins.ToList(),
            CanClose = true
        };

        OpenOrActivateTab(monacoTab);
    }

    private void OpenReflectionTab(IFlowerSeed p)
    {
        var plugin = p;
    
        // Load plugin if not loaded
        if (plugin.Status?.Results == null || !plugin.Status.Results.Any())
        {
            plugin.Main(_fileModel.FullName);
        }
    
        var pluginTab = new ReflectionTab
        {
            Title = plugin.Seed,
            Icon = "🔌",
            Status = plugin.Status!,
            CanClose = true
        };
    
        OpenOrActivateTab(pluginTab);
    }

    private void OpenOrActivateTab(WorkspaceTab tab)
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Id == tab.Id);

        if (existingTab != null)
        {
            SelectedTab = existingTab;
        }
        else
        {
            Tabs.Add(tab);
            SelectedTab = tab;
        }
    }

    private void OpenNotEmptyReflectionTabs()
    {
        foreach (var flowerSeed in AvailablePlugins.Where(f => f.Status.Results.Count > 0))
        {
            OpenReflectionTab(flowerSeed);
        }
    }
    private void CloseTab(object t)
    {
        var tab = (WorkspaceTab)t;
        if (!tab.CanClose)
            return;

        if (tab is HexViewTab h)
            h.Context?.Reader.Dispose();

        Tabs.Remove(tab);
    }
}