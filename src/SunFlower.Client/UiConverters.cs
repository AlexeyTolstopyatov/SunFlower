// CoffeeLake (C) 2026-*
// 
// The UiConverters.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Client.Model;

namespace SunFlower.Client;

public static class UiConverters
{
    public static readonly Version? KernelApiVersion = App.PluginService
        .GetVersionInfo()[0] // SunFlower.Kernel.dll metadata
        .Contracts[0].Item2; // Kernel API metadata
    
    /// <summary>
    /// Executes condition of nullity of view model. Returns true if model is null
    /// </summary>
    public static readonly FuncValueConverter<ObservableObject?, bool> ViewModelMissingConverter = new(v => v is null);
    /// <summary>
    /// Checks how flower version compares with current client app.
    /// If major version differs - returns "incompatible!" message
    /// </summary>
    public static readonly FuncValueConverter<Version, string> IsCompatConverter = new(v =>
    {
        if (KernelApiVersion is null)
            return "Bad process request";

        if (v?.Major != KernelApiVersion.Major)
            return "Incompatible. Discarded"; // Higher or lower -> not running
        
        return "Works";
    });

    public static readonly FuncValueConverter<Theme, ThemeVariant> ThemeConverter = new(v =>
    {
        return v switch
        {
            Theme.Light => ThemeVariant.Light,
            Theme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    });
}