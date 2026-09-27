//
// CoffeeLake (C) 2026-*
//
// RecentFilesViewModel - home page. Shows recent files and plugin status.
//

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Service;
using SunFlower.Kernel.Readers;

namespace SunFlower.Client.ViewModel;

public partial class RecentFilesViewModel : ObservableObject
{
    private readonly RecentFilesService _recentFilesService;
    private readonly MainWindowViewModel _mainWindow;

    /// <summary>
    /// Collection of recent files for the ListBox.
    /// </summary>
    public ObservableCollection<FlowerFileInfo> RecentFiles { get; } = [];

    /// <summary>
    /// Currently selected file in the ListBox.
    /// </summary>
    [ObservableProperty]
    private FlowerFileInfo? _selectedFile;

    /// <summary>
    /// Summary text about how many plugins are loaded.
    /// </summary>
    [ObservableProperty]
    private string _pluginStatusText = "Loading plugins…";

    public RecentFilesViewModel(RecentFilesService recentFilesService, MainWindowViewModel mainWindow)
    {
        _recentFilesService = recentFilesService;
        _mainWindow = mainWindow;

        RefreshList();

        // Plugins may not be loaded yet at this point (this VM is created in the
        // MainWindowViewModel constructor). Show a pending status instead of
        // touching FlowerCollection, which would throw during startup.
        RefreshPluginStatus();
    }

    /// <summary>
    /// Refresh the recent files list from the service.
    /// </summary>
    public void RefreshList()
    {
        RecentFiles.Clear();
        foreach (var file in _recentFilesService.Files)
        {
            RecentFiles.Add(file);
        }
    }

    /// <summary>
    /// Updates the plugin status text. Safe to call before plugins are loaded:
    /// it checks <see cref="PluginService.IsInitialized"/> and never touches
    /// <see cref="PluginService.FlowerCollection"/> while it would throw.
    /// </summary>
    public void RefreshPluginStatus()
    {
        var pluginService = _mainWindow.PluginService;

        PluginStatusText = pluginService.IsInitialized
            ? $"Plugins loaded: {pluginService.FlowerCollection.Count}"
            : "Loading plugins…";
    }

    /// <summary>
    /// Open the selected file via the main window navigator.
    /// </summary>
    [RelayCommand]
    private async Task OpenFile(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            await _mainWindow.OpenFileAsync(path);
        }
    }

    /// <summary>
    /// Open double-clicked/recently selected file.
    /// </summary>
    [RelayCommand]
    private async Task OpenSelectedFile(object? selectedItem)
    {
        SelectedFile = (FlowerFileInfo?)selectedItem;
        if (SelectedFile != null)
        {
            await _mainWindow.OpenFileAsync(SelectedFile.Path);
        }
    }
    [RelayCommand]
    private async Task RemoveSelectedFile(object? selectedItem)
    {
        SelectedFile = (FlowerFileInfo?)selectedItem;
        if (SelectedFile != null)
        {
            await _recentFilesService.RemoveAsync(SelectedFile);
        }
        
        SelectedFile = null;
        RefreshList();
    }
    /// <summary>
    /// Clear the recent files list.
    /// </summary>
    [RelayCommand]
    private void ClearRecent()
    {
        _ = _recentFilesService.ClearAsync();
        RecentFiles.Clear();
        RefreshList();
    }
}