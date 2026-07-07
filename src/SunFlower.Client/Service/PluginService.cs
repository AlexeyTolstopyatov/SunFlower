//
// CoffeeLake (C) 2026-*
//
// PluginService is a singleton service that initializes all flower seeds
// once at application startup.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.Service;

public class PluginService
{
    private readonly FluentFlowerManager _manager;

    /// <summary>
    /// Seeds that were loaded at initialization (metadata + interfaces).
    /// </summary>
    private List<FlowerSeedData>? _loadedSeeds;

    private bool _initialized;

    public PluginService()
    {
        _manager = FluentFlowerManager.CreateInstance();
        _loadedSeeds = null;
        _initialized = false;

        Initialize();
    }

    /// <summary>
    /// Initialize all plugins. Call once at application startup.
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
            return;

        var pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        if (!Directory.Exists(pluginsDirectory))
        {
            _loadedSeeds = [];
            _initialized = true;
            return;
        }

        _manager.ActivateAll();
        _loadedSeeds = _manager.Seeds.ToList();

        _initialized = true;
    }

    /// <summary>
    /// Get all loaded seeds metadata.
    /// </summary>
    public IReadOnlyList<FlowerSeedData> Seeds =>
        _loadedSeeds ?? throw new InvalidOperationException(
            "PluginService not initialized. Call Initialize() first.");

    /// <summary>
    /// Analyze a file with all loaded plugins. Returns results.
    /// Does NOT reinitialize plugins — uses cached instances.
    /// </summary>
    public IReadOnlyList<FlowerSeedData> Analyze(string filePath)
    {
        if (!_initialized)
            throw new InvalidOperationException("PluginService not initialized.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Target file not found.", filePath);

        _manager.UpdateAll(filePath);

        return _manager.Seeds.AsReadOnly();
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