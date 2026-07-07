//
// CoffeeLake (C) 2026-*
//
// RecentFilesService manages recent files list via JSON persistence.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SunFlower.Kernel.Readers;

namespace SunFlower.Client.Service;

public class RecentFilesService
{
    private readonly JsonService<FlowerFileInfo> _jsonService = new();
    private const string RecentFileName = "recent";

    /// <summary>
    /// The current recent files list.
    /// </summary>
    public IReadOnlyList<FlowerFileInfo> Files => _jsonService.Data.AsReadOnly();

    /// <summary>
    /// Load recent files from disk. Call once at startup.
    /// </summary>
    public async Task LoadAsync()
    {
        var path = GetRegistryPath();
        if (File.Exists(path))
        {
            await _jsonService.ReadAsync(path);
        }
    }

    /// <summary>
    /// Add a file to the top of the recent list and persist.
    /// </summary>
    public async Task AddAsync(FlowerFileInfo info)
    {
        _jsonService.Data.RemoveAll(f => f.Path == info.Path);
        _jsonService.Data.Insert(0, info);

        // Keep max 20 entries
        if (_jsonService.Data.Count > 20)
            _jsonService.Data.RemoveRange(20, _jsonService.Data.Count - 20);

        await _jsonService.WriteAsync(RecentFileName);
    }

    public async Task RemoveAsync(FlowerFileInfo info)
    {
        _jsonService.Data.RemoveAll(f => f.Path == info.Path);
        await _jsonService.WriteAsync(RecentFileName);
    }
    /// <summary>
    /// Clear the recent files list.
    /// </summary>
    public async Task ClearAsync()
    {
        _jsonService.Data.Clear();
        await _jsonService.WriteAsync(RecentFileName);
    }

    private static string GetRegistryPath()
    {
        var registryDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry");
        Directory.CreateDirectory(registryDir);
        return Path.Combine(registryDir, $"{RecentFileName}.json");
    }
}