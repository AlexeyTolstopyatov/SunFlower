// CoffeeLake (C) 2026-*
// 
// The DockWorkspaceViewModel.CommandPalette.cs represents part for all
// Palette-prefix named command bindings of docking workspace. 
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel.Dock;

namespace SunFlower.Client.ViewModel;

public partial class DockWorkspaceViewModel
{
    private async Task PaletteSaveProjectAsync()
    {
        if (_workspaceService.CurrentProject == null)
            return;

        try
        {
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
    }

    private async Task PaletteImportFileAsync()
    {
        if (_workspaceService.CurrentProject == null || ThisWindow is null) return;
        var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
        if (storage is null) return;

        try
        {
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
    }

    private async Task PaletteExportFileAsync(ProjectFileViewModel file)
    {
        if (_workspaceService.CurrentProject == null || ThisWindow is null) return;
        var storage = TopLevel.GetTopLevel(ThisWindow)?.StorageProvider;
        if (storage is null) return;

        try
        {
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
            await vm.LoadAsync();
            
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
            var result =
                await _dialogService.ShowDialogAsync<GoToAddressDialogViewModel, (bool IsRelative, ulong Offset)?>(vm);

            if (result is { } r) hexDoc.JumpToOffset(r.Offset, r.IsRelative);
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

        var result = await storage.SaveFilePickerWithResultAsync(new FilePickerSaveOptions
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
            await Host.OpenAsync(newPath, false);
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
        var config = WorkspaceHost.ReadFontConfigurationBlock(_settingsService.Current.TextControl);
        try
        {
            var vm = new StructLayoutDialogViewModel(
                config.Family,
                config.Size,
                ownerWindow: ThisWindow,
                selectedBytes: selectedBytes
            );
            await _dialogService.ShowDialogAsync<StructLayoutDialogViewModel, object?>(vm);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[{nameof(PaletteStructLayoutAsync)}] : {ex}");
        }
    }

    private async Task PaletteDisassembleSelectionAsync(HexDocumentViewModel _, byte[] selectedBytes)
    {
        try
        {
            var dasmService = new DisassemblingService(_workspaceService);
            var vm = new DisassemblerDialogViewModel(selectedBytes, 0, dasmService);
            await _dialogService.ShowDialogAsync<DisassemblerDialogViewModel, object?>(vm);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"{nameof(PaletteDisassembleSelectionAsync)}] : {ex}");
        }
    }

    private async Task PaletteBytesConverterAsync(HexDocumentViewModel _, byte[] selectedBytes)
    {
        var config = WorkspaceHost.ReadFontConfigurationBlock(_settingsService.Current.HexControl);
        try
        {
            var vm = new BytesDialogViewModel(
                config.Family,
                config.Size,
                selectedBytes
            );
            await _dialogService.ShowDialogAsync<BytesDialogViewModel, object?>(vm);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"{nameof(PaletteBytesConverterAsync)}] : {ex}");
        }
    }
}