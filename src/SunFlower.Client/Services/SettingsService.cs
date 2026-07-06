//
// CoffeeLake (C) 2026-*
//
// SettingsService manages application settings (UI, paths, preferences).
// Persisted as JSON in the Registry directory.
//
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SunFlower.Client.Model;

namespace SunFlower.Client.Services;

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
    private readonly string _settingsPath;
    private SettingsModel _settings;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public SettingsService()
    {
        var registryDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry");
        Directory.CreateDirectory(registryDir);
        _settingsPath = Path.Combine(registryDir, "settings.json");
        _settings = new SettingsModel();
        
        _ = LoadAsync();
    }

    /// <summary>
    /// Current settings. Read-only access from outside.
    /// Use UpdateAsync to modify.
    /// </summary>
    public SettingsModel Current => _settings;

    /// <summary>
    /// Load settings from disk. Creates defaults if file doesn't exist.
    /// </summary>
    public async Task LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            _settings = new SettingsModel();
            
            await Console.Out.WriteLineAsync("Settings file not found. Making something new");
            
            await SaveAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            _settings = JsonSerializer.Deserialize<SettingsModel>(json, JsonOptions) ?? 
                        new SettingsModel();
            
            Console.WriteLine(json);
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            _settings = new SettingsModel();
        }
    }

    /// <summary>
    /// Save current settings to disk.
    /// </summary>
    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_settings, JsonOptions);
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    /// <summary>
    /// Update a single setting and persist.
    /// </summary>
    public async Task UpdateAsync(Action<SettingsModel> update)
    {
        update(_settings);
        await SaveAsync();
    }
}