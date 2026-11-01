//
// CoffeeLake (C) 2026-*
//
// WorkspaceService manages opened files and their analysis results.
// Works together with ProjectService to support raw binaries and .flowerproj projects.
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SunFlower.Kernel.Readers;

namespace SunFlower.Client.Service;

public class WorkspaceService(PluginService pluginService, ProjectService projectService)
{
    /// <summary>
    ///     Path to the currently opened file, or null if none.
    /// </summary>
    public string? CurrentFilePath { get; private set; }

    /// <summary>
    ///     File info for the current workspace.
    /// </summary>
    public FlowerFileInfo? CurrentFileInfo { get; private set; }

    /// <summary>
    ///     Whether the current workspace is a .flowerproj project file.
    /// </summary>
    public bool IsProject { get; private set; }

    /// <summary>
    ///     Provides access to the underlying ProjectService for file management.
    /// </summary>
    public ProjectService ProjectService => projectService;

    /// <summary>
    ///     Current project info from ProjectService.
    /// </summary>
    public ProjectInfo? CurrentProject => projectService.CurrentProject;

    /// <summary>
    ///     Fires when analysis results are updated.
    /// </summary>
    public event Action? ResultsUpdated;

    /// <summary>
    ///     Open a file - automatically detects whether it's a raw binary or project.
    /// </summary>
    public async Task OpenFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);

        // Activate plugins so the workspace can list them. This is a cheap no-op once
        // plugins are already activated at startup. Opening no longer runs plugin
        // analysis - it is triggered on demand from the project file context menu.
        await pluginService.ActivateAsync();

        // Determine if it's a project or raw binary
        if (projectService.IsProjectFile(path) || ProjectService.HasProjectExtension(path))
        {
            OpenProject(path);
            return;
        }

        await OpenRawBinary(path);
    }

    /// <summary>
    ///     Open a raw binary and create a temp project for it.
    ///     When <paramref name="flowerName" /> is provided, only that plugin is run;
    ///     otherwise plugin analysis is deferred to the project file context menu.
    /// </summary>
    private async Task OpenRawBinary(string path, [Optional] string? flowerName)
    {
        projectService.OpenRawBinary(path);
        IsProject = false;
        CurrentFilePath = Path.GetFullPath(path);
        CurrentFileInfo = FlowerBinarySeeker.Get(CurrentFilePath);

        // Run a single requested flower; full analysis is deferred to the context menu.
        if (flowerName is not null)
            await pluginService.AnalyzeAsync(CurrentFilePath, flowerName);

        ResultsUpdated?.Invoke();
    }

    /// <summary>
    ///     Open a .flowerproj project file.
    ///     Plugin analysis is deferred to the project file context menu.
    /// </summary>
    private void OpenProject(string path)
    {
        var project = projectService.OpenProject(path);
        IsProject = true;

        var originalBinary = project.OriginalBinaryPath;
        CurrentFilePath = originalBinary ?? path;
        CurrentFileInfo = FlowerBinarySeeker.Get(CurrentFilePath);

        ResultsUpdated?.Invoke();
    }

    /// <summary>
    ///     Free project pointers and close project
    /// </summary>
    public void CloseFile()
    {
        CurrentFilePath = null;
        CurrentFileInfo = null;
        IsProject = false;

        projectService.CloseProject();
    }

    public async Task SaveProjectAsync(string? savePath = null)
    {
        await projectService.SaveProjectAsync(savePath);
    }
}