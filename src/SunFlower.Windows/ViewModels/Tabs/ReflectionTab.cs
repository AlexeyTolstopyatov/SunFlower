using System.Collections.ObjectModel;
using SunFlower.Abstractions;
using SunFlower.Windows.ViewModels.Tree;

namespace SunFlower.Windows.ViewModels.Tabs;

public class ReflectionTab : WorkspaceTab
{
    private FlowerSeedStatus _status;
    private ObservableCollection<ResultNode> _resultNodes;

    public FlowerSeedStatus Status
    {
        get => _status;
        set
        {
            SetField(ref _status, value);
            // Something changes? Let it grow
            ResultNodes = PluginResultsTreeBuilder.BuildTree(value.Results?.ToList() ?? []);
        }
    }

    public ObservableCollection<ResultNode> ResultNodes
    {
        get => _resultNodes;
        set => SetField(ref _resultNodes, value);
    }

    public ReflectionTab()
    {
        _resultNodes = [];
    }
}