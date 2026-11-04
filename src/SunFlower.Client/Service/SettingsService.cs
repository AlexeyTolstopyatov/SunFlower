//
// CoffeeLake (C) 2026-*
//
// SettingsService manages application settings (UI, paths, preferences).
// Persisted as JSON in the Registry directory.
//

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SunFlower.Client.Model;

namespace SunFlower.Client.Service;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly string _settingsPath;

    public SettingsService()
    {
        var registryDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry");
        Directory.CreateDirectory(registryDir);
        _settingsPath = Path.Combine(registryDir, "settings.json");
        Current = new SettingsModel();

        CacheSubdirectories =
            Directory.EnumerateDirectories(AppContext.BaseDirectory, "CacheV1").Count() +
            Directory.EnumerateDirectories(AppContext.BaseDirectory, "CacheV2").Count();

        DeleteCache();

        Task.Run(LoadAsync);
        //_ = LoadAsync();
    }

    private int CacheSubdirectories { get; }

    /// <summary>
    ///     Current settings. Read-only access from outside.
    ///     Use UpdateAsync to modify.
    /// </summary>
    public SettingsModel Current { get; private set; }

    /// <summary>
    ///     Fired whenever settings are changed (live) or saved, so open editors can
    ///     re-apply fonts, column visibility and bytes-per-line on the fly.
    /// </summary>
    public event Action? SettingsChanged;

    /// <summary>
    ///     Notifies subscribers that settings have changed and open editors should
    ///     re-apply the new values immediately.
    /// </summary>
    public void NotifySettingsChanged() => SettingsChanged?.Invoke();

    /// <summary>
    ///     Load settings from disk. Creates defaults if file doesn't exist.
    /// </summary>
    private async Task LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            Current = new SettingsModel();

            await Console.Out.WriteLineAsync("Settings file not found. Making something new");
            await SaveAsync();

            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            Current = JsonSerializer.Deserialize<SettingsModel>(json, JsonOptions) ??
                      new SettingsModel();

            // Ensure font blocks always have defaults so the UI never binds to null.
            Current.HexControl ??= new Font("Consolas", 16);
            Current.TextControl ??= new Font("Droid Sans Mono", 13);
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            Current = new SettingsModel();
        }
    }

    /// <summary>
    ///     Save current settings to disk.
    /// </summary>
    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(Current, JsonOptions);
        await File.WriteAllTextAsync(_settingsPath, json);
        SettingsChanged?.Invoke();
    }

    private void DeleteCache(string path)
    {
        if (!Directory.Exists(path))
            return;

        foreach (var directory in Directory.EnumerateDirectories(path))
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception exn)
            {
                Console.WriteLine(exn);
            }
    }

    public Task DeleteCache()
    {
        if (CacheSubdirectories == 0)
            return Task.CompletedTask;

        DeleteCache(Path.Combine(AppContext.BaseDirectory, "CacheV1"));
        DeleteCache(Path.Combine(AppContext.BaseDirectory, "CacheV2"));

        return Task.CompletedTask;
    }
}