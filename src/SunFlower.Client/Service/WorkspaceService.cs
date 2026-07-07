//
// CoffeeLake (C) 2026-*
//
// WorkspaceService manages opened files and their analysis results.
// Works together with ProjectService to support raw binaries and .flowerproj projects.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SunFlower.Kernel.Readers;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Service;

public class WorkspaceService(PluginService pluginService, ProjectService projectService)
{
    private string? _currentFilePath;
    private FlowerFileInfo? _currentFileInfo;
    private IReadOnlyList<FlowerSeedData>? _currentResults;
    private bool _isProject;

    /// <summary>
    /// Path to the currently opened file, or null if none.
    /// </summary>
    public string? CurrentFilePath => _currentFilePath;

    /// <summary>
    /// File info for the current workspace.
    /// </summary>
    public FlowerFileInfo? CurrentFileInfo => _currentFileInfo;

    /// <summary>
    /// Whether the current workspace is a .flowerproj project file.
    /// </summary>
    public bool IsProject => _isProject;

    /// <summary>
    /// Provides access to the underlying ProjectService for file management.
    /// </summary>
    public ProjectService ProjectService => projectService;

    /// <summary>
    /// Current project info from ProjectService.
    /// </summary>
    public ProjectInfo? CurrentProject => projectService.CurrentProject;

    /// <summary>
    /// Fires when analysis results are updated.
    /// </summary>
    public event Action? ResultsUpdated;

    /// <summary>
    /// Open a file - automatically detects whether it's a raw binary or project.
    /// </summary>
    public FlowerFileInfo OpenFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);

        // Determine if it's a project or raw binary
        if (projectService.IsProjectFile(path) || projectService.HasProjectExtension(path))
        {
            return OpenProject(path);
        }

        return OpenRawBinary(path);
    }

    /// <summary>
    /// Open a raw binary and create a temp project for it.
    /// </summary>
    private FlowerFileInfo OpenRawBinary(string path)
    {
        var project = projectService.OpenRawBinary(path);
        _isProject = false;
        _currentFilePath = Path.GetFullPath(path);
        _currentFileInfo = FlowerBinarySeeker.Get(_currentFilePath);

        // Analyze with all plugins
        _currentResults = pluginService.Analyze(_currentFilePath);
        ResultsUpdated?.Invoke();

        return _currentFileInfo;
    }

    /// <summary>
    /// Open a .flowerproj project file.
    /// </summary>
    private FlowerFileInfo OpenProject(string path)
    {
        var project = projectService.OpenProject(path);
        _isProject = true;

        var originalBinary = project.OriginalBinaryPath;
        _currentFilePath = originalBinary ?? path;
        _currentFileInfo = FlowerBinarySeeker.Get(_currentFilePath);

        // Analyze with all plugins
        _currentResults = pluginService.Analyze(_currentFilePath);
        ResultsUpdated?.Invoke();

        return _currentFileInfo;
    }

    /// <summary>
    /// Free project pointers and close project 
    /// </summary>
    public void CloseFile()
    {
        _currentFilePath = null;
        _currentFileInfo = null;
        _currentResults = null;
        _isProject = false;

        projectService.CloseProject();
    }

    public async Task SaveProjectAsync(string? savePath = null)
    {
        await projectService.SaveProjectAsync(savePath);
    }
}