// SunFlower.Client.ViewModel/DockWorkspaceViewModel.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
    private readonly WorkspaceLayoutFactory _factory = new();
    private readonly PluginService _pluginService;
    // ReSharper disable once NotAccessedField.Local
    private readonly SettingsService _settingsService; // TODO: possibility of dead code
    private readonly WorkspaceService _workspaceService;

    private DialogService _dialogService;

    [ObservableProperty] private IRootDock _layout;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyText = "Sun is rising for you";
    [ObservableProperty] private Window? _thisWindow;

    private const string NoFileSelectedMessage = "NO FILE SELECTED"; 

    public IWorkspaceHost Host { get; }
    public ExplorerDocumentViewModel Explorer { get; }

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

    public void SetDialogService(DialogService d) => _dialogService = d;

    /// <summary>
    /// Returns the currently active HexDocumentViewModel (if a hex tab is open), otherwise null.
    /// </summary>
    private HexDocumentViewModel? GetActiveHexDocument()
    {
        var contentDock = FindDock<IDocumentDock>(Layout, WorkspaceLayoutFactory.DocumentsId);
        return contentDock?.VisibleDockables?
            .OfType<HexDocumentViewModel>()
            .FirstOrDefault(d => d is { IsActive: true } || contentDock.ActiveDockable == d);
    }

    /// <summary>
    /// Builds the full command palette: global commands, context-sensitive commands,
    /// and plugin commands for the selected file.
    /// </summary>
    [RelayCommand]
    private async Task BuildCommandPaletteAsync()
    {
        var items = new System.Collections.Generic.List<CommandPaletteItem>();

        var hasProject = _workspaceService.CurrentProject is not null;

        if (hasProject)
        {
            items.Add(new CommandPaletteItem
            {
                Name = "[Project] Save Project As...",
                Description = "Save the current .flowerproj project",
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
                items.Add(new CommandPaletteItem()
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
            }
        }

        var file = Explorer.SelectedFile;
        if (file is not null)
        {
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
        }

        if (items.Count == 0)
            return;

        var vm = new ExecuteDialogViewModel(items);
        vm.Prepare();
        await _dialogService.ShowDialogAsync<ExecuteDialogViewModel, object?>(vm);
    }
    
    private async Task PaletteSaveProjectAsync()
    {
        if (_workspaceService.CurrentProject == null)
            return;

        try
        {
            IsBusy = true;
            BusyText = "Saving...";

            if (_workspaceService.IsProject)
            {
                await _workspaceService.SaveProjectAsync();
            }
            else
            {
                if (ThisWindow is null) 
                    return;
                var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
                
                if (storage is null) 
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

                if (file is not null)
                {
                    var path = file.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(path))
                        await _workspaceService.SaveProjectAsync(path);
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteSaveProjectAsync)}] : {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PaletteImportFileAsync()
    {
        if (_workspaceService.CurrentProject == null || ThisWindow is null) return;
        var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
        if (storage is null) return;

        try
        {
            IsBusy = true;
            BusyText = "Waiting for a file...";

            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import file into project",
                AllowMultiple = false
            });

            if (files.Count == 0) return;

            var sourcePath = files[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(sourcePath)) return;

            var destFileName = Path.GetFileName(sourcePath);
            _workspaceService.ProjectService.AddProjectFile(sourcePath, destFileName);

            await Explorer.RefreshAsync();

            var newPath = Path.Combine(_workspaceService.CurrentProject.WorkingDirectory, destFileName);
            await Host.OpenAsync(newPath);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Palette] Import file failed: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PaletteExportFileAsync(ProjectFileViewModel file)
    {
        if (_workspaceService.CurrentProject == null || ThisWindow is null) return;
        var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
        if (storage is null) return;

        try
        {
            IsBusy = true;
            BusyText = "Exporting";

            var dest = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Export \"{file.FileName}\"",
                DefaultExtension = Path.GetExtension(file.FileName),
                SuggestedFileName = file.FileName,
                FileTypeChoices =
                [
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*.*"]
                    }
                ]
            });

            if (dest is null) return;

            var destPath = dest.TryGetLocalPath();
            if (string.IsNullOrEmpty(destPath)) return;

            await _workspaceService.ProjectService.ExportProjectFileAsync(file.FileName, destPath);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteExportFileAsync)}] : {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PaletteShowFileInformationAsync()
    {
        string? path = null;

        if (Explorer.SelectedFile is { } selectedFile)
            path = selectedFile.FullPath;
        else if (_workspaceService.CurrentFilePath is { } wsPath)
            path = wsPath;

        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var vm = new FileInformationLayoutDialogViewModel(path);
            await _dialogService.ShowDialogAsync<FileInformationLayoutDialogViewModel, object?>(vm);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteShowFileInformationAsync)}] : {ex}");
        }
    }

    private async Task PaletteJumpAtASync(HexDocumentViewModel hexDoc)
    {
        if (hexDoc.Editor is null || hexDoc.BinaryDocument is null) 
            return;

        try
        {
            var maxAddress = hexDoc.BinaryDocument.Length;
            var vm = new GoToAddressDialogViewModel(maxAddress);
            var result = await _dialogService.ShowDialogAsync<GoToAddressDialogViewModel, (bool IsRelative, ulong Offset)?>(vm);
            
            if (result is { } r)
            {
                hexDoc.JumpToOffset(r.Offset, r.IsRelative);
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteJumpAtASync)} failed]: {ex}");
        }
    }

    private async Task PaletteRenameAsync()
    {
        // Confirmation dialog calling
        var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
        
        if (storage is null || Explorer.SelectedFile is null) 
            return;

        var result = await storage.SaveFilePickerWithResultAsync(new FilePickerSaveOptions()
        {
            Title = "Rename current file",
            DefaultExtension = Path.GetExtension(Explorer.SelectedFile.FileName)
        });
        
        
        if (result.File is not null)
            _workspaceService.ProjectService.RenameProjectFile(Explorer.SelectedFile.FileName, result.File.Name);

        await Explorer.RefreshAsync();
    }

    private async Task PaletteSaveSelectionAsync(HexDocumentViewModel hexDoc, byte[] selectedBytes)
    {
        if (_workspaceService.CurrentProject == null) return;

        var defaultName =
            $"__0x{hexDoc.Editor?.Selection.Range.Start.ByteIndex:X16}_0x{hexDoc.Editor?.Selection.Range.End.ByteIndex:X16}.bin";

        try
        {
            IsBusy = true;
            BusyText = "Saving...";

            await _workspaceService.ProjectService.WriteBinaryAsync(defaultName, selectedBytes);
            await Explorer.RefreshAsync();

            var newPath = Path.Combine(_workspaceService.CurrentProject.WorkingDirectory, defaultName);
            await Host.OpenAsync(newPath);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteSaveSelectionAsync)}] Save selection failed: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PaletteStructLayoutAsync(HexDocumentViewModel _, byte[] selectedBytes)
    {
        try
        {
            var vm = new StructLayoutDialogViewModel(
                fontSize: _settingsService.Current.TextControl?.Size,
                fontFamily:_settingsService.Current.TextControl?.Family,
                selectedBytes: selectedBytes,
                ownerWindow: ThisWindow
            );
            await _dialogService.ShowDialogAsync<StructLayoutDialogViewModel, object?>(vm);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteStructLayoutAsync)}] : {ex}");
        }
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