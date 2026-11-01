// SunFlower.Client.ViewModel.Dock/ProjectExplorerViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Model;
using SunFlower.Client.Service;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.ViewModel.Dock;

public partial class ExplorerDocumentViewModel : FileDocument
{
    private readonly IWorkspaceHost? _host;
    private readonly PluginService? _pluginService;
    private readonly ProjectService? _projectService;

    private bool _initialOpenPending = true;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private ProjectFileViewModel? _selectedFile;

    public ExplorerDocumentViewModel(
        IWorkspaceHost? host,
        ProjectService? projectService,
        PluginService? pluginService)
    {
        _host = host;
        _projectService = projectService;
        _pluginService = pluginService;

        Id = "__projectExplorer__";
        Title = "Project Explorer";
        CanClose = false;
        CanFloat = false;

        _ = InitializeAsync();
    }

    public ObservableCollection<ProjectFileViewModel> ProjectFiles { get; } = [];
    public ObservableCollection<FlowerData> AvailableFlowers { get; } = [];

    private async Task InitializeAsync()
    {
        try
        {
            LoadPlugins();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Explorer] init failed: {ex}");
        }
    }

    private void LoadPlugins()
    {
        AvailableFlowers.Clear();
        
        if (_pluginService is null || !_pluginService.IsInitialized) 
            return;
        
        foreach (var flower in _pluginService.FlowerCollection)
            AvailableFlowers.Add(flower);
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        var project = _projectService?.CurrentProject;
        if (project is null)
        {
            ProjectFiles.Clear();
            SelectedFile = null;
            return;
        }

        string[] files;
        try
        {
            files = await Task.Run(() => Directory.GetFiles(project.WorkingDirectory));
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Explorer] enumerate '{project.WorkingDirectory}' failed: {ex}");
            return;
        }

        var previousPath = SelectedFile;

        ProjectFiles.Clear();
        foreach (var path in files)
            ProjectFiles.Add(new ProjectFileViewModel
            {
                FileName = Path.GetFileName(path),
                FullPath = path
            });
        
        SelectedFile = previousPath is null
            ? null
            : ProjectFiles.FirstOrDefault(f => f == previousPath);

        // Init: open the first file of the freshly opened project.
        if (_initialOpenPending && SelectedFile is null && ProjectFiles.Count > 0)
        {
            SelectedFile = ProjectFiles[0];
            
            await OpenSelectedFileAsync(true);
        }

        _initialOpenPending = false;
    }

    [RelayCommand]
    private async Task OpenSelectedFileAsync(bool permanent = false)
    {
        if (SelectedFile is null || _host is null) return;
        try
        {
            await _host.OpenAsync(SelectedFile.FullPath, permanent);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Explorer] open '{SelectedFile}' failed: {ex}");
        }
    }

    [RelayCommand]
    private async Task RunPluginAsync(FlowerData? flower)
    {
        if (flower is null || SelectedFile is null || _host is null) return;

        try
        {
            IsBusy = true;
            await _host.ExecuteFlowerAsync(flower, SelectedFile.FullPath);

            // The plugin may have written a result file - refresh the tree.
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Explorer] plugin run failed: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}