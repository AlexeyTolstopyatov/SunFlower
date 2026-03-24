using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using HandyControl.Controls;
using Microsoft.Xaml.Behaviors.Core;
using SunFlower.Abstractions;
using SunFlower.Windows.Services;
using SunFlower.Windows.Views;

namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel : NotifyPropertyChanged
{
    private readonly RegistryManager _registryManager;
    private readonly WindowManager _windowManager;

    public MainWindowViewModel()
    {
        _registryManager = RegistryManager.CreateInstance();
        _windowManager = new();

        _recentTable = LoadRecentTableOnStartup();
        _loadedSeeds = [];
        _clientVersion = string.Empty;

        _getFileCommand = new ActionCommand(GetFile);
        _getRecentFileCommand = new ActionCommand(GetRecentFile);
        _getNotImplementedGrowlCommand = new ActionCommand(GetNotImplementedGrowl);
        _getRegistryFileCommand = new ActionCommand(OpenRegFileByName);
        _clearCacheCommand = new ActionCommand(ClearCache);
        _clearRecentFilesCommand = new ActionCommand(ClearRecentFiles);
        _clearRecentFileCommand = new ActionCommand(ClearRecentFile);
        _getConverterWindowCommand = new ActionCommand(_ =>
        {
            _windowManager.ShowUnmanaged(
                new ConverterWindow(),
                title: "Converter");
        });
        _getYourTableCommand = new ActionCommand(_ =>
        {
            _windowManager.ShowUnmanaged(
                windowInstance: new DataGridWindow(),
                title: "Table");
        });
        _name = string.Empty;
        _fullName = string.Empty;
        _size = string.Empty;
        _typeString = string.Empty;
        _signature = string.Empty;

        ClientVersion = "v";
        TellCurrentVersion();
    }

    private string _clientVersion;
    private ICommand _clearCacheCommand;
    private DataTable _recentTable;
    private ObservableCollection<IFlowerSeed> _loadedSeeds;

    public string ClientVersion
    {
        get => _clientVersion;
        set => SetField(ref _clientVersion, value);
    }

    public DataTable RecentTable
    {
        get => _recentTable;
        set => SetField(ref _recentTable, value);
    }

    public ICommand ClearCacheCommand
    {
        get => _clearCacheCommand;
        set => SetField(ref _clearCacheCommand, value);
    }
    public ObservableCollection<IFlowerSeed> Seeds
    {
        get => _loadedSeeds;
        set => SetField(ref _loadedSeeds, value);
    }
    private DataTable LoadRecentTableOnStartup()
    {
        DataTable result = new();
        RegistryManager.CreateInstance()
            .Of("recent")
            .Fill(ref result);

        return result;
    }

    private void ClearCache()
    {
        var target = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sunflower.Windows");

        try
        {
            if (Directory.Exists(target))
                Directory.Delete(target, true);
        }
        catch (Exception e)
        {
            Growl.ErrorGlobal(e.Message);
            Process.Start("explorer.exe", target);
        }
    }
    /// <summary>
    /// Windows Client version
    /// </summary>
    private void TellCurrentVersion()
    {
        var ver =
            FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "SunFlower.Windows.dll")
                .FileVersion ?? " undefined";

        ClientVersion += ver;
    }
}