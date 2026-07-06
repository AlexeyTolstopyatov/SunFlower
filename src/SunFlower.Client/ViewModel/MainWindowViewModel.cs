//
// CoffeeLake (C) 2026-*
//
// MainWindowViewModel - root view model for the application shell.
// Manages navigation between "pages" (Recent, Workspace).
// Injects all services that live for the lifetime of the app.
//
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Services;

namespace SunFlower.Client.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    public PluginService PluginService { get; }
    public RecentFilesService RecentFilesService { get; }
    public WorkspaceService WorkspaceService { get; }
    public ProjectService ProjectService { get; }
    public WindowService WindowService { get; }
    public Version Version { get; init; }
    public SettingsService SettingsService { get; init; }
    public ThemeService ThemeService { get; init; }

    [ObservableProperty]
    private ObservableObject? _currentPage;
    
    /// <summary>
    /// Reference to the main window for dialogs. Set by View.
    /// </summary>
    public Window? MainWindow { get; set; }

    public MainWindowViewModel(PluginService pluginService,
        RecentFilesService recentFilesService,
        WorkspaceService workspaceService,
        ProjectService projectService,
        WindowService windowService, 
        SettingsService settingsService,
        ThemeService themeService)
    {
        PluginService = pluginService;
        RecentFilesService = recentFilesService;
        WorkspaceService = workspaceService;
        ProjectService = projectService;
        WindowService = windowService;
        SettingsService = settingsService;
        ThemeService = themeService;
        
        Version = UiConverters.KernelApiVersion ?? throw new NullReferenceException("Kernel API metadata is missing");

        CurrentPage = new RecentFilesViewModel(recentFilesService, this);
    }

    /// <summary>
    /// Called once after the constructor. Initializes plugins and loads recent files.
    /// </summary>
    public async Task InitializeAsync()
    {
        PluginService.Initialize();
        await RecentFilesService.LoadAsync();

        if (CurrentPage is RecentFilesViewModel recent)
        {
            recent.RefreshList();
        }
    }

    [RelayCommand]
    private void NavigateHome()
    {
        CurrentPage = new RecentFilesViewModel(RecentFilesService, this);
    }
    [RelayCommand]
    private void NavigateDiagnostics()
    {
        CurrentPage = new PluginViewModel(PluginService);
    }
    [RelayCommand]
    private void NavigateSettings()
    {
        CurrentPage = new SettingsViewModel(SettingsService, ThemeService);
    }
    
    [RelayCommand]
    private async Task ImportFileAsync()
    {
        if (MainWindow == null)
            return;

        var storage = TopLevel.GetTopLevel(MainWindow)?.StorageProvider;
        if (storage == null)
            return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Binary File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("All Executables")
                {
                    Patterns = ["*.*", "*.exe", "*.dll", "*.sys", "*.bin", "*.rom", "*.com", "*.drv"]
                },
                new FilePickerFileType("Project Files")
                {
                    Patterns = ["*.flowerproj"]
                }
            ]
        });

        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            await OpenFileAsync(path);
        }
    }

    [RelayCommand]
    private async Task RestoreProjectAsync()
    {
        if (MainWindow == null)
            return;

        var storage = TopLevel.GetTopLevel(MainWindow)?.StorageProvider;
        if (storage == null)
            return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SunFlower Project",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("SunFlower Project")
                {
                    Patterns = ["*.flowerproj"]
                }
            ]
        });

        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();

        if (!string.IsNullOrEmpty(path))
            await OpenFileAsync(path);
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (WorkspaceService.CurrentProject == null)
            return;

        if (WorkspaceService.IsProject)
        {
            await WorkspaceService.SaveProjectAsync();
        }
        else
        {
            await SaveProjectAsAsync();
        }
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        if (MainWindow == null || WorkspaceService.CurrentProject == null)
            return;

        var storage = TopLevel.GetTopLevel(MainWindow)?.StorageProvider;
        if (storage == null)
            return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save SunFlower Project",
            DefaultExtension = ".flowerproj",
            FileTypeChoices =
            [
                new FilePickerFileType("SunFlower Project")
                {
                    Patterns = ["*.flowerproj"]
                }
            ]
        });

        if (file == null)
            return;

        var path = file.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            await WorkspaceService.SaveProjectAsync(path);
        }
    }

    [RelayCommand]
    private void DetachWorkspace()
    {
        if (CurrentPage is not WorkspaceViewModel workspaceVm) return;
        
        WindowService.OpenWorkspaceWindow(workspaceVm);
        NavigateHome();
    }

    [RelayCommand]
    private async Task OpenExternalFile(string path)
    {
        try
        {
            WorkspaceService.OpenFile(path);
            var fileInfo = WorkspaceService.CurrentFileInfo;
            if (fileInfo != null)
            {
                await RecentFilesService.AddAsync(fileInfo);
            }

            var analysisService = new PluginAnalysisService(WorkspaceService);
            WindowService.OpenWorkspaceWindow(
                new WorkspaceViewModel(WorkspaceService, PluginService, analysisService, SettingsService));
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Failed to open file: {ex}");
        }
    }

    /// <summary>
    /// Public method for opening a file path from other ViewModels.
    /// </summary>
    public async Task OpenFileAsync(string path) => await OpenExternalFile(path);

    public async Task OnApplicationExitAsync()
    {
        if (ProjectService.CurrentProject?.IsDirty == true)
        {
            try
            {
                await SaveProjectAsync();
            }
            catch
            {
                // Ignore save errors on exit
            }
        }

        ProjectService.Dispose();
    }
}