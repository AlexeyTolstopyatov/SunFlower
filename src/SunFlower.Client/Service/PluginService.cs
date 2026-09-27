//
// CoffeeLake (C) 2026-*
//
// PluginService is a singleton service that initializes all flowers
// once at application startup.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    }

    /// <summary>
    /// ActivateAsync all plugins. Call once at application startup.
    /// Plugin activation (Assembly.LoadFrom, reflection, Activator.CreateInstance) is CPU-bound
    /// and F#'s <c>task {}</c> builder runs its synchronous body on the calling thread,
    /// so the whole pipeline must be offloaded to the thread pool to keep the UI responsive.
    /// </summary>
    public async Task ActivateAsync()
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

        await Task.Run(_manager.ActivateAllAsync);

        _loaded = _manager.LoadedFlowers.ToList();

        _initialized = true;
    }

    /// <summary>
    /// Get all loaded flowers metadata.
    /// </summary>
    public IReadOnlyList<FlowerData> FlowerCollection =>
        _loaded ?? throw new InvalidOperationException(
            "PluginService not initialized. Call ActivateAsync() first.");

    /// <summary>
    /// Whether all plugins have been loaded. Until this is <c>true</c>,
    /// <see cref="FlowerCollection"/> is not available and throws.
    /// Use this to guard access during startup (see RecentFilesViewModel).
    /// </summary>
    public bool IsInitialized => _initialized;

    public string[] KernelMessages => _manager.Messages.ToArray();
    /// <summary>
    /// AnalyzeAsync a file with all loaded plugins. Returns results.
    /// Does NOT reinitialize plugins — uses cached instances.
    ///
    /// For unknown flower - recalls all flowers in collection
    /// </summary>
    public async Task AnalyzeAsync(string filePath, [Optional] string? flowerName)
    {
        if (!_initialized)
            throw new InvalidOperationException("PluginService not initialized.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Target file not found.", filePath);

        // await Task.Run(() => _manager.InitializeAllAsync(filePath));
        if (flowerName is null)
            await Task.Run(() => _manager.InitializeAllAsync(filePath));
        else
            await Task.Run(() => _manager.InitializeAsync(flowerName, filePath));
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