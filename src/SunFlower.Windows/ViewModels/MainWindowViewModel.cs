using System.Data;
using System.IO;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json;
using SunFlower.Abstractions;
using SunFlower.Services;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel : NotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        _recentTable = LoadRecentTableOnStartup();
        _loadedSeeds = LoadFlowerSeedResults();
        _statusText = string.Empty;
        
        _getFileCommand = new Command(GetFile);
        _getRecentFileCommand = new Command(GetRecentFile);
        _getProcessCommand = new Command(GetWin32Process);
        _getNotImplementedGrowlCommand = new Command(GetNotImplementedGrowl);
        _getMachineWordsCommand = new Command((o) => { OpenChildWindowByDataContext(new MachineWordsWindowViewModel()); });
        _callEditorCommand = new ActionCommand(CallEditor);
        
        Tell($"Recent files found: {_recentTable.Rows.Count}");
        Tell($"Seeds loaded: {_loadedSeeds.Count}");

        _windowsService = new WindowsService();
        _fileName = string.Empty;
        _filePath = string.Empty;
        _cpu = string.Empty;
        _signature = string.Empty;
        
        Tell("Windows service registered");
    }

    private readonly IWindowsService _windowsService;
    private DataTable _recentTable;
    private List<IFlowerSeed> _loadedSeeds;
    private string _statusText;
    
    public DataTable RecentTable
    {
        get => _recentTable;
        set => SetField(ref _recentTable, value);
    }
    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public List<IFlowerSeed> Seeds
    {
        get => _loadedSeeds;
        set => SetField(ref _loadedSeeds, value);
    }
    private DataTable LoadRecentTableOnStartup()
    {
        Tell(nameof(LoadRecentTableOnStartup));
        string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "recent.json");
        return JsonConvert.DeserializeObject<DataTable>(json)!;
    }

    private List<IFlowerSeed> LoadFlowerSeedResults()
    {
        return FlowerSeedManager
            .CreateInstance()
            .LoadAllFlowerSeeds()
            .Seeds;
    }

    private void Tell(string phrase)
    {
        string text = "-> " + phrase + "\r\n";
        StatusText += text;
    }
}