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

// /// <summary>
// /// Application settings model.
// /// </summary>
// public class AppSettings
// {
//     public string Language { get; set; } = "en";

//     public bool ShowHexViewByDefault { get; set; } = false;
//     public bool ShowLineNumbers { get; set; } = true;
//     public bool WordWrap { get; set; } = false;
//     public string FontFamily { get; set; } = "Consolas";
//     public double FontSize { get; set; } = 13.0;
//     public string Theme { get; set; } = "Dark"; // "Dark" | "Light" | "System"

//     public bool AutoSaveOnExit { get; set; } = true;
//     public bool ConfirmExit { get; set; } = true;
//     public bool AutoReAnalyze { get; set; } = false;

//     public int BinaryProbeSize { get; set; } = 8192;
//     public double BinaryThreshold { get; set; } = 0.05;
// }

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