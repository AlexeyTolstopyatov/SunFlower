//
// CoffeeLake (C) 2026-*
//
// PluginAnalysisService runs a specific plugin on the opened file,
// writes the result to the project working directory as a file,
// and returns metadata about what view was created.
//

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Service;

public class PluginContentView
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Kind { get; set; } = "Data";
    public object? RawContent { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public string ContentType { get; set; } = "Text";
    public bool IsBinary { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsAssembly { get; set; }
    public long FileSize { get; set; }
}

public class PluginAnalysisService(WorkspaceService workspaceService)
{
    /// <summary>
    /// Run a specific plugin on a given project file, write the result to the project dir.
    /// The result file is named after the file it was applied to,
    /// following the convention: <c>&lt;targetFileName&gt;_&lt;pluginName&gt;</c>
    /// (e.g. applying the plugin to "explorer.exe" gives "explorer_Sunflower.Pe.md").
    /// Fix: clears stale results before Main() so Code-plugins don't
    /// accumulate output from previous file runs.
    /// </summary>
    /// <param name="flowerData">The plugin to run.</param>
    /// <param name="targetPath">
    /// Path of the project file the plugin is applied to. When null, falls back
    /// to the original binary of the current project (or the opened file).
    /// </param>
    public async Task<PluginContentView> AnalyzeAndSaveAsync(FlowerData flowerData, string? targetPath)
    {
        var project = workspaceService.CurrentProject;
        if (project == null)
            throw new InvalidOperationException("No project is open.");

        var workingDir = project.WorkingDirectory;
        var pluginName = flowerData.Instance.Name;
        var kind = $"{flowerData.Kind}";
        var ext = kind == "Code" ? ".asm" : ".md";

        var appliedPath = targetPath ?? project.OriginalBinaryPath ?? workspaceService.CurrentFilePath;
        if (string.IsNullOrEmpty(appliedPath))
            throw new InvalidOperationException("No target file is available to analyze.");

        Exception? error = null;
        try
        {
            await flowerData.Instance.CreateAsync(appliedPath);
        }
        catch (Exception e)
        {
            error = e;
        }
        
        var content = error is not null 
            ? $"# {pluginName}\n\n**Error:** {error.Message}\n\n```\n{error}\n```" 
            : flowerData.render();

        // Write content to a file named after the analyzed target,
        // so results never collide when a plugin is applied to several files.
        var targetBaseName = Path.GetFileNameWithoutExtension(appliedPath);
        var safeFileName = $"{EraseInvalidCharacters(targetBaseName)}_{EraseInvalidCharacters(pluginName)}{ext}";
        var filePath = Path.Combine(workingDir, safeFileName);
        await File.WriteAllTextAsync(filePath, content);

        project.IsDirty = true;

        return new PluginContentView
        {
            Name = $"{targetBaseName}_{pluginName}",
            FileName = safeFileName,
            FilePath = filePath,
            ContentType = kind == "Code" ? "Assembly" : "Markdown",
            Kind = kind,
            RawContent = content,
            HasError = error is not null,
            ErrorMessage = error?.Message, // null reference?
            IsBinary = false,
            IsAssembly = kind == "Code",
            IsMarkdown = kind != "Code",
            FileSize = content.Length
        };
    }

    public static async Task<PluginContentView?> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var fileName = Path.GetFileName(filePath);
        var name = Path.GetFileNameWithoutExtension(fileName);

        var detection = ContentTypeDetector.Detect(filePath);

        if (detection.IsBinary)
        {
            var fileInfo = new FileInfo(filePath);
            return new PluginContentView
            {
                Name = name,
                FileName = fileName,
                FilePath = filePath,
                ContentType = detection.Description,
                Kind = "Binary",
                IsBinary = true,
                IsAssembly = false,
                IsMarkdown = false,
                FileSize = fileInfo.Length
            };
        }

        var content = await File.ReadAllTextAsync(filePath);
        return new PluginContentView
        {
            Name = name,
            FileName = fileName,
            FilePath = filePath,
            ContentType = detection.Description,
            Kind = detection.SubType == ContentSubType.Assembly ? "Code" : "Data",
            RawContent = content,
            IsBinary = false,
            IsAssembly = detection.SubType == ContentSubType.Assembly,
            IsMarkdown = detection.SubType == ContentSubType.Markdown,
            FileSize = content.Length
        };
    }

    private static string EraseInvalidCharacters(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return invalid.Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}