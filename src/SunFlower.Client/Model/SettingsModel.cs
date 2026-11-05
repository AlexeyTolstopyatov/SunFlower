//
// CoffeeLake (C) 2026-*
// 
// The SettingsModel.cs represents settings file model
// Uses by SettingsView to configure visuals 
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
//

using System.Text.Json.Serialization;

namespace SunFlower.Client.Model;

public enum Theme
{
    System,
    Light,
    Dark
}

public class Font(string family, double size)
{
    [JsonPropertyName("family")] public string? Family { get; set; } = family;

    [JsonPropertyName("size")] public double Size { get; set; } = size;
}

public class SettingsModel
{
    [JsonPropertyName("theme")] public Theme Theme { get; set; }
    [JsonPropertyName("text_control_theme")] public int TextControlTheme { get; set; }
    [JsonPropertyName("text_control")] public Font? TextControl { get; set; }
    [JsonPropertyName("text_show_line_numbers")] public bool ShowLineNumbers { get; set; }
    [JsonPropertyName("hex_control")] public Font? HexControl { get; set; }
    /// <summary>
    ///     Fixed number of hex bytes per line. null (or empty in the UI) means dynamic/automatic.
    /// </summary>
    [JsonPropertyName("hex_bytes_per_line")] public int? HexBytesPerLine { get; set; }
    [JsonPropertyName("hex_show_offset_column")] public bool HexShowOffsetColumn { get; set; } = true;
    [JsonPropertyName("hex_show_hex_column")] public bool HexShowHexColumn { get; set; } = true;
    [JsonPropertyName("hex_show_binary_column")] public bool HexShowBinaryColumn { get; set; } = true;
    [JsonPropertyName("hex_show_ascii_column")] public bool HexShowAsciiColumn { get; set; }
    /// <summary>
    ///     Whether dialog windows are shown with an animation. Can be disabled.
    /// </summary>
    [JsonPropertyName("dialog_animation_enabled")] public bool DialogAnimationEnabled { get; set; } = true;

    /// <summary>
    ///     Global hotkey for the command palette (e.g. "Ctrl+Shift+P").
    /// </summary>
    [JsonPropertyName("command_palette_hotkey")] public string CommandPaletteHotkey { get; set; } = "Ctrl+Shift+P";
}