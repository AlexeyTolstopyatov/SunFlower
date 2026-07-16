//
// CoffeeLake (C) 2026-*
//
// WorkspaceViewModel is the main workspace when a file is opened.
// Three separate strongly-typed fields for each content viewer:
//   - ActiveContentText (:string) -> MarkdownViewer
//   - AssemblyDocument (:TextDocument) -> AvaloniaEdit
//   - ActiveBinaryDocument (:IBinaryDocument) -> HexEditor
//

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaHex;
using AvaloniaHex.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Service;
using SunFlower.Client.View;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.ViewModel;

public enum ViewMode
{
    Hex,
    Text,
    Html
}

public partial class WorkspaceViewModel : ObservableObject
{
    [ObservableProperty]
    private WorkspaceService _workspaceService;
    private readonly PluginService _pluginService;
    private readonly PluginAnalysisService _analysisService;
    private readonly DisassemblingService _disassemblingService;
    private DialogService _dialogService;

    public void SetDialogService(DialogService dialogService)
    {
        _dialogService = dialogService;
    }
    
    private Window? _thisWindow;
    private ulong _targetAddress;

    #region File information

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileType = string.Empty;

    [ObservableProperty]
    private string _fileSize = string.Empty;

    [ObservableProperty]
    private string _fileSignature = string.Empty;

    [ObservableProperty]
    private string _projectDirectory = string.Empty;

    #endregion

    #region Plugin results

    [ObservableProperty]
    private PluginResultItem? _selectedPluginResult;

    [ObservableProperty]
    private string _selectedResultContent = string.Empty;

    #endregion

    #region Project files (left top)

    public ObservableCollection<ProjectFileItem> ProjectFiles { get; } = [];

    [ObservableProperty]
    private ProjectFileItem? _selectedProjectFile;

    #endregion

    #region Available plugins (left bottom)

    public ObservableCollection<FlowerSeedData> AvailablePlugins { get; } = [];

    [ObservableProperty]
    private FlowerSeedData? _selectedPlugin;

    [ObservableProperty]
    private PluginActionItem? _selectedPluginAction;

    #endregion

    #region Active content (three separate typed fields)

    /// <summary>Markdown content.</summary>
    [ObservableProperty]
    private string _activeContentText = string.Empty;

    /// <summary>Code content</summary>
    [ObservableProperty]
    private TextDocument? _activeTextDocument;

    /// <summary>Binary content</summary>
    [ObservableProperty]
    private IBinaryDocument? _activeBinaryDocument;

    /// <summary>Show the Markdown viewer.</summary>
    [ObservableProperty]
    private bool _isMarkdownView;

    /// <summary>Show the code editor.</summary>
    [ObservableProperty]
    private bool _isAssemblyView;

    /// <summary>Show the hex editor</summary>
    [ObservableProperty]
    private bool _isBinaryView;

    #endregion

    #region Disassembler

    [ObservableProperty]
    private bool _isDisassemblyInProgress;

    [ObservableProperty]
    private DisassemblerArchitecture _selectedArchitecture = DisassemblerArchitecture.I8086;

    public Array AvailableArchitectures { get; } =
        Enum.GetValues(typeof(DisassemblerArchitecture));

    #endregion

    /// <summary>
    /// Name of the file that is currently open in the active viewer,
    /// so we know which file to save back.
    /// </summary>
    private string? _activeFileName;

    /// <summary>
    /// Raw bytes of the currently open binary file (for save-back).
    /// </summary>
    private byte[]? _activeBinaryBytes;

    [ObservableProperty]
    private bool _hasHexSelection;

    [ObservableProperty]
    private FontFamily _hexFontFamily;

    [ObservableProperty]
    private FontFamily _textFontFamily;

    [ObservableProperty]
    private double _hexFontSize;

    [ObservableProperty]
    private double _textFontSize;

    public WorkspaceViewModel(
        WorkspaceService workspaceService,
        PluginService pluginService,
        PluginAnalysisService analysisService,
        SettingsService settingsService)
    {
        _workspaceService = workspaceService;
        _pluginService = pluginService;
        _analysisService = analysisService;
        _disassemblingService = App.DisassemblingService;
        
        LoadFileInfo();
        LoadProjectFiles();
        LoadAvailablePlugins();

        HexFontFamily  = FontFamily.Parse(settingsService.Current.HexControl?.Family!);
        HexFontSize = settingsService.Current.HexControl?.Size ?? 16;
        
        TextFontFamily = FontFamily.Parse(settingsService.Current.TextControl?.Family!);
        TextFontSize = settingsService.Current.TextControl?.Size ?? 16;
        
        _workspaceService.ResultsUpdated += OnResultsUpdated;
    }

    public Window? ThisWindow
    {
        set => _thisWindow = value;
    }

    [RelayCommand]
    private void ChangeView(object? mode)
    {
        switch ((ViewMode?)mode)
        {
            case ViewMode.Hex:
                IsBinaryView = true;
                IsAssemblyView = false;
                IsMarkdownView = false;
                break;
            case ViewMode.Text:
                IsBinaryView = false;
                IsAssemblyView = true;
                IsMarkdownView = false;
                break;
            case ViewMode.Html:
                IsBinaryView = false;
                IsAssemblyView = false;
                IsMarkdownView = true;
                break;
            default:
                IsAssemblyView = false;
                IsMarkdownView = false;
                IsBinaryView = false;
                break;
        }
    }

    private void LoadFileInfo()
    {
        var info = WorkspaceService.CurrentFileInfo;
        if (info == null) return;

        FileName = info.Name;
        FileType = info.Type;
        FileSize = $"{info.Size:F1} KB";
        FileSignature = info.Sign;

        var project = WorkspaceService.CurrentProject;
        ProjectDirectory = project?.WorkingDirectory ?? string.Empty;
    }

    private void LoadProjectFiles()
    {
        var project = WorkspaceService.CurrentProject;
        if (project == null) return;

        var dir = project.WorkingDirectory;
        if (!Directory.Exists(dir)) return;

        var originalName = project.OriginalBinaryName;

        var files = Directory.GetFiles(dir)
            .Select(fp => new FileInfo(fp))
            .Select(fi => new ProjectFileItem
            {
                Name = fi.Name,
                Path = fi.FullName,
                Size = fi.Length > 1024
                    ? $"{fi.Length / 1024.0:F1} KB"
                    : $"{fi.Length} B",
                Extension = fi.Extension.ToLowerInvariant(),
                IsOriginalBinary = string.Equals(
                    fi.Name, originalName, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        ProjectFiles.Clear();
        foreach (var item in files)
        {
            ProjectFiles.Add(item);
        }
    }

    private void LoadAvailablePlugins()
    {
        AvailablePlugins.Clear();

        try
        {
            foreach (var seed in _pluginService.Seeds)
            {
                AvailablePlugins.Add(seed);
            }
        }
        catch
        {
            // PluginService not initialized yet
        }
    }

    private void OnResultsUpdated()
    {
        LoadFileInfo();
        LoadProjectFiles();
    }

    partial void OnSelectedPluginResultChanged(PluginResultItem? value)
    {
        if (value != null)
        {
            SelectedResultContent = value.HasError
                ? value.ErrorMessage!
                : value.HasResults ? "Works Correct" : "No results";
        }
    }

    partial void OnSelectedProjectFileChanged(ProjectFileItem? value)
    {
        if (value != null)
        {
            _ = OpenProjectFileAsync(value);
        }
    }

    partial void OnSelectedPluginActionChanged(PluginActionItem? value)
    {
        if (value != null && SelectedPlugin != null)
        {
            _ = ExecutePluginActionAsync(SelectedPlugin);
        }
    }

    [RelayCommand]
    private async Task RunSelectedPluginAsync()
    {
        if (SelectedPlugin == null) 
            return;
        
        await ExecutePluginActionAsync(SelectedPlugin);
    }

    [RelayCommand]
    private async Task ExtractHexSelectionAsync(object? parameter)
    {
        if (_activeBinaryBytes == null){
            return;
        }
        var project = WorkspaceService.CurrentProject;

        if (project == null)
            return;

        var editorApi = (HexEditor?)parameter;

        if (editorApi == null)
            return;

        var start = editorApi.Selection.Range.Start.ByteIndex;
        var end = editorApi.Selection.Range.End.ByteIndex;

        if (editorApi.Selection.Range.IsEmpty)
            return;

        ulong Abs(ulong a, ulong b) => a > b ? a - b : b - a;

        var length = Abs(start, end);
        ArrayExtension.ExtractBytes(start, length, _activeBinaryBytes, out var extracted);
        
        if (_thisWindow == null || WorkspaceService.CurrentProject == null)
            return;

        var storage = TopLevel.GetTopLevel(_thisWindow)?.StorageProvider;
        if (storage == null)
            return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Binary selection. Set the file extension manually",
            DefaultExtension = "*.*",
            FileTypeChoices =
            [
                new FilePickerFileType("Binary")
                {
                    Patterns = ["*.*"]
                }
            ]
        });
        if (file == null) 
            return;

        await WorkspaceService.ProjectService.WriteBinaryAsync(file.Name, extracted);

        project.IsDirty = true;

        LoadProjectFiles();
    }

    [RelayCommand]
    private async Task OpenProjectFileAsync(ProjectFileItem? file)
    {
        if (file == null)
            return;

        var content = await PluginAnalysisService.LoadFromFileAsync(file.Path);
        if (content is null) return;

        _activeFileName = file.Name;

        if (content.IsBinary)
        {
            var bytes = await File.ReadAllBytesAsync(content.FilePath);
            _activeBinaryBytes = bytes;
            ActiveBinaryDocument = new MemoryBinaryDocument(bytes);
            ActiveContentText = string.Empty;
            ActiveTextDocument = null;

            IsBinaryView = true;
            IsAssemblyView = false;
            IsMarkdownView = false;
        }
        else if (content.IsAssembly)
        {
            _activeBinaryBytes = null;
            var text = content.RawContent?.ToString() ?? string.Empty;
            ActiveTextDocument = new TextDocument(text);
            ActiveContentText = string.Empty;
            ActiveBinaryDocument = null;

            IsAssemblyView = true;
            IsBinaryView = false;
            IsMarkdownView = false;
        }
        else
        {
            _activeBinaryBytes = null;
            ActiveContentText = content.RawContent?.ToString() ?? string.Empty;
            ActiveTextDocument = null;
            ActiveBinaryDocument = null;

            IsMarkdownView = true;
            IsAssemblyView = false;
            IsBinaryView = false;
        }
    }

    [RelayCommand]
    private async Task SaveActiveFileAsync()
    {
        var project = WorkspaceService.CurrentProject;
        if (project == null || _activeFileName == null)
            return;

        var filePath = Path.Combine(project.WorkingDirectory, _activeFileName);
        if (!File.Exists(filePath))
            return;

        if (IsBinaryView && _activeBinaryBytes != null)
        {
            await File.WriteAllBytesAsync(filePath, _activeBinaryBytes);
        }
        else if (IsAssemblyView && ActiveTextDocument != null)
        {
            await File.WriteAllTextAsync(filePath, ActiveTextDocument.Text);
        }
        else if (IsMarkdownView)
        {
            await File.WriteAllTextAsync(filePath, ActiveContentText);
        }

        project.IsDirty = true;
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (_thisWindow == null || WorkspaceService.CurrentProject == null)
            return;

        var storage = TopLevel.GetTopLevel(_thisWindow)?.StorageProvider;
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
    private async Task OpenSelectedProjectFileAsync()
    {
        if (SelectedProjectFile != null)
        {
            await OpenProjectFileAsync(SelectedProjectFile);
        }
    }

    [RelayCommand]
    private async Task DeleteProjectFileAsync(ProjectFileItem? file)
    {
        if (file == null)
            return;

        try
        {
            var pService = WorkspaceService.ProjectService;
            if (pService.CurrentProject == null)
                return;

            pService.DeleteProjectFile(file.Name);

            LoadProjectFiles();

            if (string.Equals(_activeFileName, file.Name, StringComparison.OrdinalIgnoreCase))
            {
                _activeFileName = null;
                _activeBinaryBytes = null;
                ActiveContentText = string.Empty;
                ActiveTextDocument = null;
                ActiveBinaryDocument = null;
                IsBinaryView = false;
                IsAssemblyView = false;
                IsMarkdownView = false;
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Cannot delete file: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RenameProjectFileAsync(ProjectFileItem? file)
    {
        if (file == null || _thisWindow == null)
            return;

        var newName = await ShowRenameDialogAsync(file.Name);
        if (string.IsNullOrWhiteSpace(newName) || newName == file.Name)
            return;

        try
        {
            var pService = WorkspaceService.ProjectService;
            if (pService.CurrentProject == null)
                return;

            pService.RenameProjectFile(file.Name, newName);

            if (string.Equals(_activeFileName, file.Name, StringComparison.OrdinalIgnoreCase))
            {
                _activeFileName = newName;
            }

            LoadProjectFiles();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Cannot rename file: {ex.Message}");
        }
    }

    private async Task<string?> ShowRenameDialogAsync(string currentName)
    {
        var storage = TopLevel.GetTopLevel(_thisWindow)?.StorageProvider;
        if (storage == null)
            return null;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Rename '{currentName}'",
            SuggestedFileName = currentName,
            FileTypeChoices =
            [
                new FilePickerFileType("All Files")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (file == null)
            return null;

        var path = file.TryGetLocalPath();
        if (string.IsNullOrEmpty(path))
            return null;

        return Path.GetFileName(path);
    }

    #region Disassembler commands

    /// <summary>
    /// Shows the disassembler dialog for the entire currently open binary file.
    /// </summary>
    [RelayCommand]
    private async Task ShowDisassemblerDialogAsync()
    {
        if (_activeBinaryBytes == null)
            return;

        var dialogVm = new DisassemblerDialogViewModel(
            _activeBinaryBytes, 
            null, // disassemble all bytes
            _disassemblingService,
            SelectedArchitecture
        );

        var result = await _dialogService.ShowDialogAsync<DisassemblerDialogViewModel, string?>(dialogVm);

        if (!string.IsNullOrEmpty(result))
        {
            _activeFileName = null;
            ActiveTextDocument = new TextDocument(result);
            ActiveContentText = string.Empty;
            ActiveBinaryDocument = null;

            IsAssemblyView = true;
            IsBinaryView = false;
            IsMarkdownView = false;
        }
    }

    /// <summary>
    /// Shows the disassembler dialog for the currently selected range in the hex editor.
    /// Selection must not be empty.
    /// </summary>
    [RelayCommand]
    private async Task ShowDisassemblerSelectionDialogAsync(object? parameter)
    {
        if (_activeBinaryBytes == null || _dialogService == null)
            return;

        var editorApi = (HexEditor?)parameter;
        if (editorApi == null || editorApi.Selection.Range.IsEmpty)
            return;

        var start = (int)editorApi.Selection.Range.Start.ByteIndex;

        var dialogVm = new DisassemblerDialogViewModel(
            _activeBinaryBytes, 
            start, // disassemble with offset
            _disassemblingService,
            SelectedArchitecture
        );

        var result = await _dialogService.ShowDialogAsync<DisassemblerDialogViewModel, string?>(dialogVm);

        if (!string.IsNullOrEmpty(result))
        {
            _activeFileName = null;
            ActiveTextDocument = new TextDocument(result);
            ActiveContentText = string.Empty;
            ActiveBinaryDocument = null;

            IsAssemblyView = true;
            IsBinaryView = false;
            IsMarkdownView = false;
        }
    }
    #endregion

    #region Go To Address command

    /// <summary>
    /// Shows the "Go to Address" dialog for navigating in the hex editor.
    /// If HexEditor is not open, opens the Hex view.
    /// </summary>
    [RelayCommand]
    private async Task ShowGoToAddressDialogAsync()
    {
        if (_activeBinaryBytes == null)
            return;

        // Switch to Hex view if not already visible
        if (!IsBinaryView)
        {
            IsBinaryView = true;
            IsAssemblyView = false;
            IsMarkdownView = false;
        }

        var maxAddress = (ulong)_activeBinaryBytes.Length;
        var dialogVm = new GoToAddressDialogViewModel(maxAddress);

        var result = await _dialogService.ShowDialogAsync<GoToAddressDialogViewModel, ulong?>(dialogVm);

        // Store the result for potential use
        if (result.HasValue)
        {
            _targetAddress = result.Value;
        }
    }

    #endregion

    private async Task ExecutePluginActionAsync(FlowerSeedData seed)
    {
        var content = await _analysisService.AnalyzeAndSaveAsync(seed);

        if (content.IsAssembly)
        {
            _activeBinaryBytes = null;
            ActiveTextDocument = new TextDocument(content.RawContent?.ToString() ?? string.Empty);
            ActiveContentText = string.Empty;
            ActiveBinaryDocument = null;

            IsAssemblyView = true;
            IsBinaryView = false;
            IsMarkdownView = false;
        }
        else
        {
            _activeBinaryBytes = null;
            ActiveContentText = content.RawContent?.ToString() ?? string.Empty;
            ActiveTextDocument = null;
            ActiveBinaryDocument = null;

            IsMarkdownView = true;
            IsAssemblyView = false;
            IsBinaryView = false;
        }

        _activeFileName = content.FileName;

        LoadProjectFiles();
    }
}

#region Supporting types

public class ProjectFileItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;

    public bool IsOriginalBinary { get; set; }

    public bool CanDelete => !IsOriginalBinary;
}

public class PluginActionItem
{
    public string Name { get; set; } = string.Empty;
}

public class PluginResultItem
{
    public string PluginName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool HasError { get; init; }
    public string? ErrorMessage { get; set; } = string.Empty;
    public bool HasResults { get; init; }
}

#endregion