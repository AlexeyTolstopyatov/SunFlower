using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using HandyControl.Controls;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json;
using SunFlower.Abstractions;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel : NotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        _recentTable = LoadRecentTableOnStartup();
        _loadedSeeds = [];
        _statusText = string.Empty;
        
        _getFileCommand = new Command(GetFile);
        _getRecentFileCommand = new Command(GetRecentFile);
        _getProcessCommand = new Command(GetWin32Process);
        _getNotImplementedGrowlCommand = new Command(GetNotImplementedGrowl);
        _getMachineWordsCommand = new Command((o) => { OpenChildWindowByDataContext(new MachineWordsWindowViewModel()); });
        _callEditorCommand = new ActionCommand(CallEditor);
        _clearCacheCommand = new ActionCommand(ClearCache);
        _clearRecentFilesCommand = new ActionCommand(ClearRecentFiles);
        _clearRecentFileCommand = new ActionCommand(ClearRecentFile);
        
        Tell($"Recent files found: {_recentTable.Rows.Count}");
        
        _windowsService = new WindowsService();
        _fileName = string.Empty;
        _filePath = string.Empty;
        _cpu = string.Empty;
        _signature = string.Empty;
        
        Tell("Windows service registered");
        TellCurrentAbstractionsVersion();
    }
    
    private readonly IWindowsService _windowsService;
    private DataTable _recentTable;
    private List<IFlowerSeed> _loadedSeeds;
    private string _statusText;
    private ICommand _clearCacheCommand;

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

    public ICommand ClearCacheCommand
    {
        get => _clearCacheCommand;
        set => SetField(ref _clearCacheCommand, value);
    }
    public List<IFlowerSeed> Seeds
    {
        get => _loadedSeeds;
        set => SetField(ref _loadedSeeds, value);
    }
    private DataTable LoadRecentTableOnStartup()
    {
        Tell(nameof(LoadRecentTableOnStartup));
        string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Registry\\recent.json");
        return JsonConvert.DeserializeObject<DataTable>(json)!;
    }
    
    private void Tell(string phrase)
    {
        string text = "-> " + phrase + "\r\n";
        StatusText += text;
    }

    private void ClearCache()
    {
        string target = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sunflower.Windows",
            "WebView2Cache");

        try
        {
            if (Directory.Exists(target))
                Directory.Delete(target);
        }
        catch (Exception e)
        {
            Growl.ErrorGlobal(e.Message);
            Process.Start("explorer.exe", target);
        }
    }

    /// <summary>
    /// Plugins have specified interface which helps to communicate
    /// with main loader module.
    /// This method shows information about installed
    /// foundation DLL version.
    /// </summary>
    private void TellCurrentAbstractionsVersion()
    {
        string abstractionsVer =
            FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "SunFlower.Abstractions.dll")
                .FileVersion ?? "NOT FOUND";
        
        Tell("Installed abstractions: " + abstractionsVer);
    }
}