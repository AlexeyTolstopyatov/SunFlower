//
// CoffeeLake (C) 2026-*
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
//

using System;
using System.Collections.Generic;
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

namespace SunFlower.Client.ViewModel;

public partial class DockWorkspaceViewModel : ObservableObject
{
    private const string NoFileSelectedMessage = "NO FILE SELECTED";
    private readonly WorkspaceLayoutFactory _factory = new();
    private readonly PluginService _pluginService;
    private readonly SettingsService _settingsService;
    private readonly WorkspaceService _workspaceService;
    private DialogService _dialogService;
    
    [ObservableProperty] private string _busyText = "Sun is rising for you";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private IRootDock _layout;
    [ObservableProperty] private Window? _thisWindow;

    public DockWorkspaceViewModel(
        WorkspaceService workspaceService,
        PluginService pluginService,
        PluginAnalysisService analysisService,
        SettingsService settingsService)
    {
        _workspaceService = workspaceService;
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

    public IWorkspaceHost Host { get; }
    public ExplorerDocumentViewModel Explorer { get; }

    public void SetDialogService(DialogService d)
    {
        _dialogService = d;
    }

    /// <summary>
    ///     Returns the currently active HexDocumentViewModel (if a hex tab is open), otherwise null.
    /// </summary>
    private HexDocumentViewModel? GetActiveHexDocument()
    {
        var contentDock = FindDock<IDocumentDock>(Layout, WorkspaceLayoutFactory.DocumentsId);
        return contentDock?.VisibleDockables?
            .OfType<HexDocumentViewModel>()
            .FirstOrDefault(d => d is { IsActive: true } || contentDock.ActiveDockable == d);
    }

    /// <summary>
    ///     Builds the full command palette: global commands, context-sensitive commands,
    ///     and plugin commands for the selected file.
    /// </summary>
    [RelayCommand]
    private async Task BuildCommandPaletteAsync()
    {
        var items = new List<CommandPaletteItem>();

        var hasProject = _workspaceService.CurrentProject is not null;

        if (hasProject)
        {
            items.Add(new CommandPaletteItem
            {
                Name = "[Project] Save Project As...",
                Description = "Save the current project",
                Action = PaletteSaveProjectAsync
            });

            items.Add(new CommandPaletteItem
            {
                Name = "[Project] Import",
                Description = "Import an external file into the current project",
                Action = PaletteImportFileAsync
            });

            if (Explorer.SelectedFile is { } selectedFile)
            {
                items.Add(new CommandPaletteItem
                {
                    Name = $"[File] Export \"{selectedFile.FileName}\"",
                    Description = "Export the selected file from the project to an external location",
                    Action = async () => await PaletteExportFileAsync(selectedFile)
                });
                items.Add(new CommandPaletteItem
                {
                    Name = $"[File] Rename \"{selectedFile.FileName}\"",
                    Description = "Call save dialog and move current file back with new name",
                    Action = PaletteRenameAsync
                });
            }

            items.Add(new CommandPaletteItem
            {
                Name = "[Project] File Information",
                Description = Explorer.SelectedFile is { } sf
                    ? $"Show details for '{sf.FileName}'"
                    : NoFileSelectedMessage,
                Action = PaletteShowFileInformationAsync
            });
        }

        var hexDoc = GetActiveHexDocument();
        if (hexDoc is not null)
        {
            items.Add(new CommandPaletteItem
            {
                Name = "[Hex] Jump At...",
                Description = "Jump to a specific byte offset in the Hex editor",
                Action = async () => await PaletteJumpAtASync(hexDoc)
            });

            var selectedBytes = hexDoc.GetSelectedBytes();
            if (selectedBytes is { Length: > 0 })
            {
                items.Add(new CommandPaletteItem
                {
                    Name = "[$Hex] Save Selection",
                    Description = $"Save [{selectedBytes.Length} selected bytes] as a new project file",
                    Action = async () => await PaletteSaveSelectionAsync(hexDoc, selectedBytes)
                });

                items.Add(new CommandPaletteItem
                {
                    Name = "[$Hex] Struct Layout...",
                    Description = $"Reinterpret [{selectedBytes.Length} bytes] as a packed struct",
                    Action = async () => await PaletteStructLayoutAsync(hexDoc, selectedBytes)
                });

                items.Add(new CommandPaletteItem
                {
                    Name = "[$Hex] Disassemble Selection...",
                    Description = $"Disassemble [{selectedBytes.Length} selected bytes]",
                    Action = async () => await PaletteDisassembleSelectionAsync(hexDoc, selectedBytes)
                });

                items.Add(new CommandPaletteItem
                {
                    Name = "[$Hex] Bytes Converter...",
                    Description = $"Convert [{selectedBytes.Length} selected bytes] to text/integers",
                    Action = async () => await PaletteBytesConverterAsync(hexDoc, selectedBytes)
                });
            }
        }

        var file = Explorer.SelectedFile;
        if (file is not null)
            items.AddRange(_pluginService.FlowerCollection
                .Select(f => new CommandPaletteItem
                {
                    Name = $"[File|Plug] {f.Instance.Name}",
                    Description = $"Run {f.Instance.Name} on {file.FileName}",
                    Action = async () =>
                    {
                        await Host.ExecuteFlowerAsync(f, file.FullPath);
                        await Explorer.RefreshAsync();
                    }
                }));

        if (items.Count == 0)
            return;

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