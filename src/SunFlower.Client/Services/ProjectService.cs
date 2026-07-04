//
// CoffeeLake (C) 2026-*
//
// ProjectService manages SunFlower project files.
// Projects are ZIP archives with a 32-byte binary header.
// When a file is opened, ProjectService determines whether it's
// a raw binary or a project file and acts accordingly.
//
// Structure:
// - Header (32 bytes): magic "FLOWERED" + version + timestamps + originalFileSize
// - Body: ZIP archive containing analysis results, user notes, etc.
//
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SunFlower.Client.Services;

/// <summary>
/// Represents an opened project state.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Raw binary path if opened from a binary file, or project file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// True if this is a .flowerproj project file (not raw binary).
    /// </summary>
    public bool IsProject { get; set; }

    /// <summary>
    /// Path to the temp working directory for this project.
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Whether the project has unsaved changes.
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Original binary file path (for projects only).
    /// </summary>
    public string? OriginalBinaryPath { get; set; }
}

public class ProjectService
{
    private static readonly byte[] MagicBytes = "FLOWERED"u8.ToArray();

    private ProjectInfo? _currentProject;
    private bool _disposed;

    /// <summary>
    /// Current project info, or null if no project is open.
    /// </summary>
    public ProjectInfo? CurrentProject => _currentProject;

    /// <summary>
    /// Checks if a file is a SunFlower project.
    /// Reads first 8 bytes and checks magic.
    /// </summary>
    public bool IsProjectFile(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            using var stream = File.OpenRead(path);
            if (stream.Length < 32)
                return false;

            var header = new byte[8];
            stream.ReadExactly(header, 0, 8);

            return !MagicBytes.Where((t, i) => header[i] != t).Any();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a file is a raw binary (not a project file).
    /// </summary>
    public bool IsRawBinary(string path)
    {
        return File.Exists(path) && !IsProjectFile(path);
    }

    /// <summary>
    /// Check if path has .flowerproj extension.
    /// </summary>
    public bool HasProjectExtension(string path) =>
        Path.GetExtension(path).Equals(".flowerproj", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Open a raw binary file: create a temp project for it.
    /// </summary>
    public ProjectInfo OpenRawBinary(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);

        // Create a unique temp directory for this project
        var tempDir = Path.Combine(AppContext.BaseDirectory, "Temp",
            $"{Path.GetFileNameWithoutExtension(path)}_{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempDir);

        // Copy the original binary into the working directory
        var binaryInWorkingDir = Path.Combine(tempDir, Path.GetFileName(path));
        File.Copy(path, binaryInWorkingDir, overwrite: true);

        _currentProject = new ProjectInfo
        {
            FilePath = path,
            IsProject = false,
            WorkingDirectory = tempDir,
            IsDirty = false,
            OriginalBinaryPath = binaryInWorkingDir
        };

        return _currentProject;
    }

    /// <summary>
    /// Open a .flowerproj project file: extract to temp directory.
    /// </summary>
    public ProjectInfo OpenProject(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Project file not found.", path);

        if (!IsProjectFile(path))
            throw new InvalidDataException("File is not a valid SunFlower project.");

        // Directory names must be unique
        var tempDir = Path.Combine(Path.GetTempPath(), "SunFlower",
            $"{Path.GetFileNameWithoutExtension(path)}_{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempDir);

        using var fileStream = File.OpenRead(path);
        var headerBytes = new byte[32];
        fileStream.ReadExactly(headerBytes, 0, 32);

        // The rest is a ZIP archive
        const long zipStart = 32;
        fileStream.Seek(zipStart, SeekOrigin.Begin);

        // Save ZIP to a temp file and extract
        var zipPath = Path.Combine(tempDir, "__body.zip");
        using (var zipStream = File.Create(zipPath))
        {
            fileStream.CopyTo(zipStream);
        }

        ZipFile.ExtractToDirectory(zipPath, tempDir, overwriteFiles: true);
        File.Delete(zipPath);

        // Find the original binary inside the working directory
        var binaryFiles = Directory.GetFiles(tempDir);
        string? originalBinary = null;

        // The largest file is likely the original binary
        foreach (var f in binaryFiles)
        {
            if (Path.GetExtension(f).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                continue;
            if (new FileInfo(f).Length > 1024)
                originalBinary = f;
        }

        _currentProject = new ProjectInfo
        {
            FilePath = path,
            IsProject = true,
            WorkingDirectory = tempDir,
            IsDirty = false,
            OriginalBinaryPath = originalBinary ?? binaryFiles[0]
        };

        return _currentProject;
    }

    /// <summary>
    /// Write a text file into the project working directory.
    /// </summary>
    public async Task WriteTextAsync(string fileName, string content)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        var fullPath = Path.Combine(_currentProject.WorkingDirectory, fileName);
        await File.WriteAllTextAsync(fullPath, content);
        _currentProject.IsDirty = true;
    }

    /// <summary>
    /// Write binary data into the project working directory.
    /// </summary>
    public async Task WriteBinaryAsync(string fileName, byte[] content)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        var fullPath = Path.Combine(_currentProject.WorkingDirectory, fileName);
        await File.WriteAllBytesAsync(fullPath, content);
        _currentProject.IsDirty = true;
    }

    /// <summary>
    /// Close the current project and clean up temp files.
    /// </summary>
    public void CloseProject()
    {
        if (_currentProject?.WorkingDirectory != null && Directory.Exists(_currentProject.WorkingDirectory))
        {
            try
            {
                Directory.Delete(_currentProject.WorkingDirectory, recursive: true);
            }
            catch
            {
                // Temp files cleanup is best-effort
            }
        }

        _currentProject = null;
    }
    /// <summary>
    /// Save the project to a .flowerproj file. If path is null,
    /// overwrite the original project file.
    /// </summary>
    public async Task SaveProjectAsync(string? savePath = null)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        var outputPath = savePath ?? _currentProject.FilePath;

        var zipPath = Path.Combine(_currentProject.WorkingDirectory, "__body.zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(_currentProject.WorkingDirectory, zipPath);

        var zipBytes = await File.ReadAllBytesAsync(zipPath);
        File.Delete(zipPath);

        await using var stream = File.Create(outputPath);
        await using var writer = new BinaryWriter(stream);

        writer.Write(MagicBytes);
        
        var now = DateTime.UtcNow;
        var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timestamp = (ulong)(now - epoch).TotalSeconds;
        var fmtVer = Kernel.Services.FlowerCompatibility.GetForAllList()[0].Version;
        
        writer.Write(fmtVer.Major); // major version
        writer.Write(fmtVer.Minor); // revision
        writer.Write(timestamp); // created
        writer.Write(timestamp); // modified

        var originalFile = _currentProject.OriginalBinaryPath;
        if (originalFile is not null && File.Exists(originalFile))
        {
            writer.Write(new FileInfo(originalFile).Length);
        }
        else
        {
            writer.Write((ulong)0);
        }

        writer.Write(zipBytes);

        _currentProject.IsDirty = false;
        _currentProject.FilePath = outputPath;
        _currentProject.IsProject = true;

        if (string.IsNullOrEmpty(Path.GetExtension(outputPath)) ||
            !Path.GetExtension(outputPath).Equals(".flowerproj", StringComparison.OrdinalIgnoreCase))
        {
            _currentProject.FilePath = outputPath;
        }
    }
    
    public void Dispose()
    {
        if (_disposed) 
            return;
        
        CloseProject();
        _disposed = true;
    }
}