// SunFlower.Client.ViewModel/DockWorkspaceViewModel.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using SunFlower.Client.Model;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel.Dock;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.ViewModel;

public partial class DockWorkspaceViewModel : ObservableObject
{
    private readonly WorkspaceLayoutFactory _factory = new();
    private readonly PluginService _pluginService;
    private readonly SettingsService _settingsService;

    private DialogService? _dialogService;

    [ObservableProperty] private IRootDock _layout;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyText = "Sun is rising for you";
    [ObservableProperty] private Window? _thisWindow;

    public IWorkspaceHost Host { get; }
    public ExplorerDocumentViewModel Explorer { get; }

    public DockWorkspaceViewModel(
        WorkspaceService workspaceService,
        PluginService pluginService,
        PluginAnalysisService analysisService,
        SettingsService settingsService)
    {
        _pluginService = pluginService;
        _settingsService = settingsService;
        
        _dialogService = new DialogService(_thisWindow!);
        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);

        var contentDock = FindDock<IDocumentDock>(Layout, WorkspaceLayoutFactory.DocumentsId) ?? 
                          throw new InvalidOperationException("ContentDock missing from layout");

        var fe = new FlowerExecutable(analysisService);
        Host = new WorkspaceHost(_factory, contentDock, fe, settingsService);
        

        // Compose explorer, host it in the left dock
        Explorer = new ExplorerDocumentViewModel(
            Host,
            workspaceService.ProjectService,
            _pluginService
        );

        var leftTool = FindDock<IDock>(Layout, WorkspaceLayoutFactory.LeftToolId) ?? 
                       throw new InvalidOperationException("LeftTool missing from layout");

        leftTool.VisibleDockables!.Clear();
        leftTool.VisibleDockables.Add(Explorer);
        leftTool.ActiveDockable = Explorer;
    }

    public void SetDialogService(DialogService d) => _dialogService = d;

    [RelayCommand]
    private async Task ShowCommandPaletteAsync()
    {
        if (_dialogService is null) return;

        var file = Explorer.SelectedFile;
        if (file is null) return;

        var items = _pluginService.FlowerCollection
            .Select(f => new CommandPaletteItem
            {
                Name = f.Instance.Name,
                Description = $"Run {f.Instance.Name} on {file.FileName}",
                Action = async () => await Host.ExecuteFlowerAsync(f, file.FullPath)
            })
            .ToList();

        var vm = new ExecuteDialogViewModel(items);
        vm.Prepare();
        await _dialogService.ShowDialogAsync<ExecuteDialogViewModel, object?>(vm);
    }

    private static T? FindDock<T>(IDock? dock, string id) where T : class, IDockable
    {
        if (dock is T match && string.Equals(dock.Id, id, StringComparison.Ordinal))
            return match;

        return dock?.VisibleDockables?
            .OfType<IDock>()
            .Select(child => FindDock<T>(child, id))
            .FirstOrDefault(x => x is not null);
    }
}