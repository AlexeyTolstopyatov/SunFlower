// CoffeeLake (C) 2026-*
// 
// The SettingsModel.cs represents settings file model
// Uses by SettingsView to configure visuals 
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.Model;

public enum Theme
{
    System,
    Light,
    Dark
}

public class Font
{
    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("size")] 
    public double Size { get; set; } = 10.0;
}

public partial class SettingsModel : ObservableObject
{
    [JsonPropertyName("theme")]
    [ObservableProperty] 
    private Theme _theme;

    [JsonPropertyName("text_control")] 
    [ObservableProperty]
    public Font? _textControl;

    [JsonPropertyName("hex_control")]
    [ObservableProperty]
    public Font? _hexControl;
}