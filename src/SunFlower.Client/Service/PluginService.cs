//
// CoffeeLake (C) 2026-*
//
// PluginService is a singleton service that initializes all flowers
// once at application startup.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Service;

public class PluginService
{
    private readonly FluentFlowerManager _manager;

    /// <summary>
    /// FlowerCollection that were loaded at initialization (metadata + interfaces).
    /// </summary>
    private List<FlowerData>? _loaded;
    
    private bool _initialized;

    public PluginService()
    {
        _manager = FluentFlowerManager.CreateInstance();
        _loaded = null;
        _initialized = false;

        InitializeAsync().Wait();
    }

    /// <summary>
    /// InitializeAsync all plugins. Call once at application startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var pluginsDirectory = Path.Combine(AppContext.BaseDirectory, "Plugins");

        if (!Directory.Exists(pluginsDirectory))
        {
            _loaded = [];
            _initialized = true;
            return;
        }

        await _manager.ActivateAllAsync();
        
        _loaded = _manager.LoadedFlowers.ToList();

        _initialized = true;
    }

    /// <summary>
    /// Get all loaded flowers metadata.
    /// </summary>
    public IReadOnlyList<FlowerData> FlowerCollection =>
        _loaded ?? throw new InvalidOperationException(
            "PluginService not initialized. Call InitializeAsync() first.");

    public string[] KernelMessages => _manager.Messages.ToArray();
    /// <summary>
    /// AnalyzeAsync a file with all loaded plugins. Returns results.
    /// Does NOT reinitialize plugins — uses cached instances.
    /// </summary>
    public async Task AnalyzeAsync(string filePath)
    {
        if (!_initialized)
            throw new InvalidOperationException("PluginService not initialized.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Target file not found.", filePath);

        await _manager.InitializeAllAsync(filePath);
    }

    /// <summary>
    /// Get compatibility info for all installed plugins.
    /// </summary>
    public IReadOnlyList<FlowerVersionInfo> GetVersionInfo()
    {
        return FlowerCompatibility
            .GetForAllList()
            .AsReadOnly();
    }
}