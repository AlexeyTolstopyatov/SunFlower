//
// CoffeeLake (C) 2026-*
//
// ThemeService — centralizes theme switching across the application.
// Fires ThemeChanged when the theme changes.
// MainWindow subscribes and sets RequestedThemeVariant programmatically.
//
using System;
using Avalonia;
using Avalonia.Styling;
using SunFlower.Client.Model;

namespace SunFlower.Client.Services;

public class ThemeService
{
    private ThemeVariant _currentVariant = ThemeVariant.Default;

    /// <summary>
    /// Fires when theme changes.
    /// </summary>
    public event Action<ThemeVariant>? ThemeChanged;

    /// <summary>
    /// Current theme variant.
    /// </summary>
    public ThemeVariant CurrentVariant => _currentVariant;

    /// <summary>
    /// Cast enum into ThemeVariant and apply changes 
    /// </summary>
    public void SetTheme(Theme theme)
    {
        var variant = theme switch
        {
            Theme.Light => ThemeVariant.Light,
            Theme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        if (_currentVariant == variant) 
            return;
        
        _currentVariant = variant;
        ThemeChanged?.Invoke(variant);
    }
}