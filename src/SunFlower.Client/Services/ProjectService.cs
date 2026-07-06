//
// CoffeeLake (C) 2026-*
// 
// ProjectService manages SunFlower project files.
// Projects are ZIP archives with a 40-byte binary header
// (matching SunFlower.Kernel.Database.FlowerObjectHeader),
// followed by a variable-length UTF-8 encoded original file name,
// followed by the ZIP archive body.
//
// .flowerproj format:
//   [0..7]   Magic "FLOWERED"        (8 bytes)
//   [8..11]  Format major version    (4 bytes)
//   [12..15] Format minor version    (4 bytes)
//   [16..23] Created timestamp       (8 bytes)
//   [24..31] Modified timestamp      (8 bytes)
//   [32..39] Container size (ZIP)    (8 bytes)
//   [40..41] FileName length         (2 bytes)
//   [42..]   Original file name      (UTF-8)
//   [..]     ZIP archive body
//
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using HandyControl.Tools.Converter;

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

    /// <summary>
    /// Original binary file name (without directory).
    /// Stored inside .flowerproj to avoid a punch-method discovery.
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Returns the file name of the original binary, or empty.
    /// </summary>
    public string OriginalBinaryName => Path.GetFileName(OriginalBinaryPath ?? string.Empty);
}

public class ProjectService
{
    private static readonly byte[] MagicBytes = "FLOWERED"u8.ToArray();

    /// <summary>
    /// Matches FlowerObjectHeader layout
    /// </summary>
    private const int HeaderSize = 40;

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
            if (stream.Length < HeaderSize)
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
    /// Checks if a file is a raw binary (not a project file).
    /// </summary>
    public bool IsRawBinary(string path)
    {
        return File.Exists(path) && !IsProjectFile(path);
    }

    /// <summary>
    /// Checks if path has .flowerproj extension.
    /// </summary>
    public bool HasProjectExtension(string path) =>
        Path.GetExtension(path).Equals(".flowerproj", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Opens a raw binary file: create a temp project for it.
    /// The original file name is preserved so it can be stored
    /// later in the .flowerproj header.
    /// </summary>
    public ProjectInfo OpenRawBinary(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);

        // Create a unique temp directory for this project
        var tempDir = Path.Combine(AppContext.BaseDirectory, "CacheV1",
            $"{Path.GetFileNameWithoutExtension(path)}_{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempDir);

        // Copy the original binary into the working directory,
        // preserving its original file name
        var originalName = Path.GetFileName(path);
        var binaryInWorkingDir = Path.Combine(tempDir, originalName);
        File.Copy(path, binaryInWorkingDir, overwrite: true);

        _currentProject = new ProjectInfo
        {
            FilePath = path,
            IsProject = false,
            WorkingDirectory = tempDir,
            IsDirty = false,
            OriginalBinaryPath = binaryInWorkingDir,
            OriginalFileName = originalName
        };

        return _currentProject;
    }

    /// <summary>
    /// Opens a [.flowerproj] project file: extract to temp directory.
    /// Reads the original file name from the header, then finds it
    /// in the extracted files by exact name match.
    /// Falls back to heuristic (largest file > 1 KB) for legacy projects.
    /// </summary>
    public ProjectInfo OpenProject(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Project file not found.", path);

        if (!IsProjectFile(path))
            throw new InvalidDataException("File is not a valid project.");

        // Directory names must be unique
        var tempDirectory = Path.Combine(AppContext.BaseDirectory, "CacheV2",
            $"{Path.GetFileNameWithoutExtension(path)}_{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempDirectory);

        using var fileStream = File.OpenRead(path);

        // Read the 40-byte FlowerObjectHeader
        var headerBytes = new byte[HeaderSize];
        fileStream.ReadExactly(headerBytes, 0, HeaderSize);

        // Read the original file name: ushort(length) + UTF-8 string
        var nameLenBytes = new byte[2];
        fileStream.ReadExactly(nameLenBytes, 0, 2);
        var nameLen = BitConverter.ToUInt16(nameLenBytes, 0);

        var originalFileName = nameLen > 0
            ? Encoding.UTF8.GetString(fileStream.ReadExactBytes(nameLen))
            : null;

        // The rest is the ZIP archive
        // Save ZIP to a temp file and extract
        var zipPath = Path.Combine(tempDirectory, "__body.zip");
        using (var zipStream = File.Create(zipPath))
        {
            fileStream.CopyTo(zipStream);
        }

        ZipFile.ExtractToDirectory(zipPath, tempDirectory, overwriteFiles: true);
        File.Delete(zipPath);

        // Locate the original binary by exact file name from header
        string? originalBinary = null;

        if (originalFileName != null)
        {
            var exactPath = Path.Combine(tempDirectory, originalFileName);
            if (File.Exists(exactPath))
            {
                originalBinary = exactPath;
            }
        }

        // Fallback for legacy projects (no file name stored):
        // pick the largest file > 1 KB
        if (originalBinary == null)
        {
            var binaryFiles = Directory.GetFiles(tempDirectory);
            foreach (var f in binaryFiles)
            {
                if (Path.GetExtension(f).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (new FileInfo(f).Length > 1024)
                {
                    originalBinary = f;
                    break;
                }
            }

            // Last resort: just use the first file
            originalBinary ??= Directory.GetFiles(tempDirectory).FirstOrDefault();
        }

        _currentProject = new ProjectInfo
        {
            FilePath = path,
            IsProject = true,
            WorkingDirectory = tempDirectory,
            IsDirty = false,
            OriginalBinaryPath = originalBinary,
            OriginalFileName = originalFileName ?? Path.GetFileName(originalBinary)
        };

        return _currentProject;
    }

    /// <summary>
    /// Writes a text file into the project working directory.
    /// </summary>
    public async Task WriteTextAsync(string fileName, string content)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        var fullPath = Path.Combine(_currentProject.WorkingDirectory, fileName);
        await File.WriteAllTextAsync(fullPath, content);
        _currentProject.IsDirty = true;
    }
    public async Task WriteBinaryAsAsync(Window windowHost)
    {
        
    }
    /// <summary>
    /// Writes binary data into the project working directory.
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
    /// Returns true if the given file name (relative to working directory)
    /// is the original binary.
    /// </summary>
    public bool IsOriginalBinaryFile(string fileName)
    {
        if (_currentProject == null)
            return false;

        var currentName = Path.GetFileName(_currentProject.OriginalBinaryPath);
        return string.Equals(currentName, fileName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Deletes a file from the project working directory.
    /// Throws if attempting to delete the original binary.
    /// </summary>
    public void DeleteProjectFile(string fileName)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        if (IsOriginalBinaryFile(fileName))
            throw new InvalidOperationException(
                $"Cannot delete the original binary file '{fileName}'. " +
                "This file is the primary analysis target.");

        var fullPath = Path.Combine(_currentProject.WorkingDirectory, fileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File '{fileName}' not found in the project.");

        File.Delete(fullPath);
        _currentProject.IsDirty = true;
    }

    /// <summary>
    /// Renames a file in the project working directory.
    /// If the file is the original binary, updates OriginalFileName
    /// and OriginalBinaryPath accordingly.
    /// Returns the new full path of the renamed file.
    /// </summary>
    public string RenameProjectFile(string oldFileName, string newFileName)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        if (string.IsNullOrWhiteSpace(newFileName))
            throw new ArgumentException("New file name cannot be empty.");

        if (newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException($"New file name contains invalid characters.");

        var oldPath = Path.Combine(_currentProject.WorkingDirectory, oldFileName);
        var newPath = Path.Combine(_currentProject.WorkingDirectory, newFileName);

        if (!File.Exists(oldPath))
            throw new FileNotFoundException($"File '{oldFileName}' not found in the project.");

        if (File.Exists(newPath))
            throw new InvalidOperationException($"A file named '{newFileName}' already exists.");

        File.Move(oldPath, newPath);

        // If the renamed file is the original binary, update the reference
        if (IsOriginalBinaryFile(oldFileName))
        {
            _currentProject.OriginalBinaryPath = newPath;
            _currentProject.OriginalFileName = newFileName;
        }

        _currentProject.IsDirty = true;
        return newPath;
    }

    /// <summary>
    /// Closes the current project and clean up cache.
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
    /// Saves the project to a .flowerproj file. If path is null,
    /// overwrite the original project file.
    ///
    /// The temporary ZIP file is created in the system temp folder
    /// (outside the working directory) to avoid file-locking conflicts
    /// between ZipFile.CreateFromDirectory iterating over working directory
    /// files and simultaneously writing the ZIP archive into the same folder.
    /// </summary>
    public async Task SaveProjectAsync(string? savePath = null)
    {
        if (_currentProject == null)
            throw new InvalidOperationException("No project is open.");

        var outputPath = savePath ?? _currentProject.FilePath;

        // Create ZIP in the system temp folder, NOT inside WorkingDirectory.
        var tempZipDir = Path.Combine(Path.GetTempPath(), "SunFlower", "SaveTemp");
        Directory.CreateDirectory(tempZipDir);
        var zipPath = Path.Combine(tempZipDir, $"__body_{Guid.NewGuid():N}.zip");

        try
        {
            ZipFile.CreateFromDirectory(_currentProject.WorkingDirectory, zipPath);

            var zipBytes = await File.ReadAllBytesAsync(zipPath);

            await using var stream = File.Create(outputPath);
            await using var writer = new BinaryWriter(stream);

            writer.Write(MagicBytes); // 8 bytes

            var now = DateTime.UtcNow;
            var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = (ulong)(now - epoch).TotalSeconds;
            var fmtVer = Kernel.Services.FlowerCompatibility.GetForAllList()[0].Version;

            writer.Write(fmtVer.Major);                 // 4 bytes -> version
            writer.Write(fmtVer.Minor);                 // 4 bytes -> revision
            writer.Write(timestamp);                    // 8 bytes -> created
            writer.Write(timestamp);                    // 8 bytes -> modified
            writer.Write((ulong)zipBytes.Length);       // 8 bytes -> containerSize

            var fileName = _currentProject.OriginalFileName ?? string.Empty;
            var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            writer.Write((ushort)fileNameBytes.Length); // 2 bytes
            writer.Write(fileNameBytes);                // bytes...

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
        finally
        {
            // Always clean up the temporary ZIP file
            if (File.Exists(zipPath))
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
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

internal static class StreamExtensions
{
    /// <summary>
    /// Reads exactly count bytes from the stream and returns them as a byte array.
    /// Throws if the stream ends prematurely.
    /// </summary>
    public static byte[] ReadExactBytes(this Stream stream, int count)
    {
        var buffer = new byte[count];
        var offset = 0;
        while (offset < count)
        {
            var read = stream.Read(buffer, offset, count - offset);
            if (read == 0)
                throw new EndOfStreamException($"Unexpected end of stream @0x{stream.Position} (read {count - offset}/{count})");
            offset += read;
        }
        return buffer;
    }
}