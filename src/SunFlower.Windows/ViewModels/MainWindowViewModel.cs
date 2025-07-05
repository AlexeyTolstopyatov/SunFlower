using System.Data;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.Win32;
using Newtonsoft.Json;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Services;

namespace SunFlower.Windows.ViewModels;

public class MainWindowViewModel : ViewModel
{
    public MainWindowViewModel()
    {
        _recentTable = LoadRecentTableOnStartup();
        _loadedSeeds = LoadFlowerSeedResultsOnStartup();
        _statusText = string.Empty;
        Tell($"Recent files found: {_recentTable.Rows.Count}");
        Tell($"Seeds loaded: {_loadedSeeds.Count}");
    }
    
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

    private List<IFlowerSeed> LoadFlowerSeedResultsOnStartup()
    {
        return FlowerSeedManager
            .CreateInstance()
            .LoadAllFlowerSeeds()
            .Seeds;
    }

    private void Tell(string phrase)
    {
        string text = "--> " + phrase + "\r\n";
        
        StatusText += text;
    }

    #region Menu Callbacks

    /// <summary>
    /// Calls <see cref="OpenFileDialog"/> instance and,
    /// Starts common reader (remembers general characteristics)
    /// and saves it to <c>recent.json</c>
    /// </summary>
    private void GetFile()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Catch image by filename",
            Filter = "All files (*.*)|*.*"
        };
        
    }
    /// <summary>
    /// Experimental feature (try to catch process by ID/Name)
    /// Requires Administrator permissions
    /// </summary>
    private void GetWin32Process()
    {
        Growl.WarningGlobal("Administrator permissions required. Not implemented yet!");
    }
    
    #endregion
}