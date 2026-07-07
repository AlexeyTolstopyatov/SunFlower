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

public class PluginAnalysisService
{
    private readonly WorkspaceService _workspaceService;

    public PluginAnalysisService(WorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    /// <summary>
    /// Run a specific plugin on the open file, write result to project dir.
    /// Fix: clears stale results before Main() so Code-plugins don't
    /// accumulate output from previous file runs.
    /// </summary>
    public async Task<PluginContentView> AnalyzeAndSaveAsync(FlowerSeedData seedData)
    {
        var project = _workspaceService.CurrentProject;
        if (project == null)
            throw new InvalidOperationException("No project is open.");

        var workingDir = project.WorkingDirectory;
        var pluginName = seedData.seed.Seed;
        var kind = $"{seedData.kind}";
        var ext = kind == "Code" ? ".asm" : ".md";

        // Clear previous results
        // CRITICAL: Code-plugins append via results.Add() instead of
        // setting the whole list. Without this Clear, old data from
        // the previous file leaks into the current output.
        var status = seedData.seed.Status;
        status.Results.Clear();
        status.LastError = null;

        var targetPath = project.OriginalBinaryPath ?? _workspaceService.CurrentFilePath;
        if (targetPath != null)
            seedData.seed.Main(targetPath);

        var hasError = status.LastError != null;
        var errorMessage = status.LastError?.Message;

        string content;
        if (hasError)
        {
            content = $"# {pluginName}\n\n**Error:** {errorMessage}\n\n```\n{status.LastError}\n```";
        }
        else if (status.Results.Count > 0)
        {
            content = seedData.render();
        }
        else
        {
            content = $"# {pluginName}\n\nNo results returned. (Blocks count=0)";
        }

        // Write content to new* file
        var safeFileName = EraseInvalidCharacters(pluginName) + ext;
        var filePath = Path.Combine(workingDir, safeFileName);
        await File.WriteAllTextAsync(filePath, content);

        project.IsDirty = true;

        return new PluginContentView
        {
            Name = pluginName,
            FileName = safeFileName,
            FilePath = filePath,
            ContentType = kind == "Code" ? "Assembly" : "Markdown",
            Kind = kind,
            RawContent = content,
            HasError = hasError,
            ErrorMessage = errorMessage,
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