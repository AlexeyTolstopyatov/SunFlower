// CoffeeLake (C) 2026-*
//
// DockingWorkspaceViewModel - VS Code style docking workspace using Dock.Avalonia.
// Provides dockable documents (Markdown, Hex, Text) and tool windows
// (Project Explorer, Kernel Messages) in a flexible layout.
// Opens via MainWindow navigation or CommandPalette (Ctrl+Shift+P).
//

using System;
using System.Collections.Generic;
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
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using SunFlower.Client.Service;
using SunFlower.Client.Model;

namespace SunFlower.Client.ViewModel;

/// <summary>
/// Base class for dockable documents. Provides Id, Title, CanClose.
/// </summary>
public abstract class DockableDocumentViewModel : Document
{
    protected DockableDocumentViewModel(string id, string title)
    {
        Id = id;
        Title = title;
    }
}

/// <summary>
/// Markdown document dockable.
/// </summary>
public partial class MarkdownDocumentViewModel : DockableDocumentViewModel
{
    [ObservableProperty] private string _content = string.Empty;
    public MarkdownDocumentViewModel() : base("markdown", "Markdown") { }
}

/// <summary>
/// Hex/binary document dockable.
/// </summary>
public partial class HexDocumentViewModel : DockableDocumentViewModel
{
    [ObservableProperty] private IBinaryDocument? _binaryDocument;
    [ObservableProperty] private FontFamily _hexFontFamily = new("Consolas");
    [ObservableProperty] private double _hexFontSize = 14;
    public HexDocumentViewModel() : base("hex", "Hex") { }
}

/// <summary>
/// Text/assembly document dockable (AvaloniaEdit).
/// </summary>
public partial class TextDocumentViewModel : DockableDocumentViewModel
{
    [ObservableProperty] private TextDocument? _textDocument;
    [ObservableProperty] private FontFamily _textFontFamily = new("Consolas");
    [ObservableProperty] private double _textFontSize = 14;
    public TextDocumentViewModel() : base("text", "Disassembly") { }
}

/// <summary>
/// Base class for dockable tool windows.
/// </summary>
public abstract class DockableToolViewModel : Tool
{
    protected DockableToolViewModel(string id, string title)
    {
        Id = id;
        Title = title;
    }
}

/// <summary>
/// Project explorer tool window.
/// </summary>
public partial class ProjectExplorerToolViewModel : DockableToolViewModel
{
    public ObservableCollection<ProjectFileItem> ProjectFiles { get; } = [];
    [ObservableProperty] private ProjectFileItem? _selectedProjectFile;
    public ProjectExplorerToolViewModel() : base("projectExplorer", "Project Explorer") { }
}

/// <summary>
/// Kernel messages tool window.
/// </summary>
public partial class KernelMessagesToolViewModel : DockableToolViewModel
{
    public ObservableCollection<string> Messages { get; } = [];
    public KernelMessagesToolViewModel() : base("kernelMessages", "Kernel Messages") { }
}

// ============================================================================
// Main DockingWorkspaceViewModel
// ============================================================================

public partial class DockingWorkspaceViewModel : ObservableObject
{
    private readonly Factory _factory = new();
    private readonly WorkspaceService _workspaceService;
    private readonly PluginService _pluginService;
    private readonly PluginAnalysisService _analysisService;
    private readonly SettingsService _settingsService;
    private DialogService? _dialogService;

    private Window? _thisWindow;

    private readonly MarkdownDocumentViewModel _markdownDoc = new();
    private readonly HexDocumentViewModel _hexDoc = new();
    private readonly TextDocumentViewModel _textDoc = new();
    private readonly ProjectExplorerToolViewModel _projectExplorer = new();
    private readonly KernelMessagesToolViewModel _kernelMessages = new();

    public IRootDock? Layout { get; private set; }

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyText = "Working...";

    public DockingWorkspaceViewModel(
        WorkspaceService workspaceService,
        PluginService pluginService,
        PluginAnalysisService analysisService,
        SettingsService settingsService)
    {
        _workspaceService = workspaceService;
        _pluginService = pluginService;
        _analysisService = analysisService;
        _settingsService = settingsService;

        _hexDoc.HexFontFamily = FontFamily.Parse(_settingsService.Current.HexControl?.Family!);
        _hexDoc.HexFontSize = _settingsService.Current.HexControl?.Size ?? 14;
        _textDoc.TextFontFamily = FontFamily.Parse(_settingsService.Current.TextControl?.Family!);
        _textDoc.TextFontSize = _settingsService.Current.TextControl?.Size ?? 14;

        BuildLayout();
        _workspaceService.ResultsUpdated += OnResultsUpdated;
    }

    public void SetDialogService(DialogService dialogService) => _dialogService = dialogService;

    public Window? ThisWindow
    {
        set => _thisWindow = value;
    }

    private void BuildLayout()
    {
        Layout = _factory.CreateLayout();

        var documentDock = new DocumentDock
        {
            VisibleDockables = _factory.CreateList<IDockable>(_markdownDoc, _hexDoc, _textDoc),
            ActiveDockable = _markdownDoc
        };

        var root = _factory.CreateRootDock();
        root.LeftPinnedDockables.Add(_projectExplorer);
        root.RightPinnedDockables.Add(_kernelMessages);
        root.VisibleDockables = _factory.CreateList<IDockable>(documentDock);

        _factory.InitLayout(Layout);
    }

    #region Commands

    [RelayCommand]
    private void ShowMarkdown()
    {
        ActivateDocument(_markdownDoc);
    }

    [RelayCommand]
    private void ShowHex()
    {
        ActivateDocument(_hexDoc);
    }

    [RelayCommand]
    private void ShowText()
    {
        ActivateDocument(_textDoc);
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        if (_thisWindow is null) return;
        var storage = TopLevel.GetTopLevel(_thisWindow)?.StorageProvider;
        if (storage is null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        var file = files.FirstOrDefault();
        var path = file?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            await _workspaceService.OpenFile(path);
            OnResultsUpdated();
        }
    }

    private void ActivateDocument(IDockable doc)
    {
        if (Layout is not null && Layout.VisibleDockables is { } docks)
        {
            foreach (var dock in docks.OfType<DocumentDock>())
            {
                dock.ActiveDockable = doc;
                break;
            }
        }
    }

    #endregion

    private void OnResultsUpdated()
    {
        _projectExplorer.ProjectFiles.Clear();
        var project = _workspaceService.CurrentProject;
        if (project != null)
        {
            var dir = project.WorkingDirectory;
            if (Directory.Exists(dir))
            {
                var originalName = project.OriginalBinaryName;
                foreach (var fi in Directory.GetFiles(dir).Select(fp => new FileInfo(fp)))
                {
                    _projectExplorer.ProjectFiles.Add(new ProjectFileItem
                    {
                        Name = fi.Name,
                        Path = fi.FullName,
                        Size = fi.Length > 1024 ? $"{fi.Length / 1024.0:F1} KB" : $"{fi.Length} B",
                        Extension = fi.Extension.ToLowerInvariant(),
                        IsOriginalBinary = string.Equals(fi.Name, originalName, StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
        }

        _kernelMessages.Messages.Clear();
        foreach (var msg in _pluginService.KernelMessages)
            _kernelMessages.Messages.Add(msg);
    }
}

#region Supporting types

// ProjectFileItem is defined in WorkspaceViewModel.cs and shared.

#endregion
