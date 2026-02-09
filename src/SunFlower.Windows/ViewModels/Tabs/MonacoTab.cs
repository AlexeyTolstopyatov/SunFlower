using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;

namespace SunFlower.Windows.ViewModels.Tabs;


public class MonacoTab : WorkspaceTab
{
    public List<IFlowerSeed> Plugins { get; init; }
}