//
// CoffeeLake (C) 2026-*
//
// WorkspaceViewModel is the main workspace when a file is opened.
// Three separate strongly-typed fields for each content viewer:
//   - ActiveContentText (string) -> MarkdownViewer
//   - AssemblyDocument (TextDocument) -> AvaloniaEdit
//   - ActiveBinaryDocument (IBinaryDocument) -> HexEditor
//

using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using AvaloniaHex.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Services;
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
    private readonly WorkspaceService _workspaceService;
    private readonly PluginService _pluginService;
    private readonly PluginAnalysisService _analysisService;

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

    #region Active content — three separate typed fields
    
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

    public WorkspaceViewModel(
        WorkspaceService workspaceService,
        PluginService pluginService,
        PluginAnalysisService analysisService)
    {
        _workspaceService = workspaceService;
        _pluginService = pluginService;
        _analysisService = analysisService;

        LoadFileInfo();
        LoadProjectFiles();
        LoadAvailablePlugins();

        _workspaceService.ResultsUpdated += OnResultsUpdated;
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
        var info = _workspaceService.CurrentFileInfo;
        if (info == null) return;

        FileName = info.Name;
        FileType = info.Type;
        FileSize = $"{info.Size:F1} KB";
        FileSignature = info.Sign;

        var project = _workspaceService.CurrentProject;
        ProjectDirectory = project?.WorkingDirectory ?? string.Empty;
    }

    private void LoadProjectFiles()
    {
        ProjectFiles.Clear();
        var project = _workspaceService.CurrentProject;
        if (project == null) return;

        var dir = project.WorkingDirectory;
        if (!Directory.Exists(dir)) return;

        foreach (var filePath in Directory.GetFiles(dir))
        {
            var info = new FileInfo(filePath);
            ProjectFiles.Add(new ProjectFileItem
            {
                Name = info.Name,
                Path = info.FullName,
                Size = info.Length > 1024
                    ? $"{info.Length / 1024.0:F1} KB"
                    : $"{info.Length} B",
                Extension = info.Extension.ToLowerInvariant()
            });
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
        if (SelectedPlugin == null) return;
        await ExecutePluginActionAsync(SelectedPlugin);
    }

    [RelayCommand]
    private async Task OpenProjectFileAsync(ProjectFileItem? file)
    {
        if (file == null)
            return;

        var content = await PluginAnalysisService.LoadFromFileAsync(file.Path);
        if (content is null) return;
        
        if (content.IsBinary)
        {
            var bytes = await File.ReadAllBytesAsync(content.FilePath);
            ActiveBinaryDocument = new MemoryBinaryDocument(bytes);
            ActiveContentText = string.Empty;
            ActiveTextDocument = null;

            IsBinaryView = true;
            IsAssemblyView = false;
            IsMarkdownView = false;
        }
        else if (content.IsAssembly)
        {
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
            ActiveContentText = content.RawContent?.ToString() ?? string.Empty;
            ActiveTextDocument = null;
            ActiveBinaryDocument = null;

            IsMarkdownView = true;
            IsAssemblyView = false;
            IsBinaryView = false;
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

    private async Task ExecutePluginActionAsync(FlowerSeedData seed)
    {
        var content = await _analysisService.AnalyzeAndSaveAsync(seed);
        
        if (content.IsAssembly)
        {
            ActiveTextDocument = new TextDocument(content.RawContent?.ToString() ?? string.Empty);
            ActiveContentText = string.Empty;
            ActiveBinaryDocument = null;

            IsAssemblyView = true;
            IsBinaryView = false;
            IsMarkdownView = false;
        }
        else
        {
            ActiveContentText = content.RawContent?.ToString() ?? string.Empty;
            ActiveTextDocument = null;
            ActiveBinaryDocument = null;

            IsMarkdownView = true;
            IsAssemblyView = false;
            IsBinaryView = false;
        }

        // Refresh project files list (new file was created)
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