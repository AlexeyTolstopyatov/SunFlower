//
// CoffeeLake (C) 2026-*
//
// ThemeService — centralizes theme switching across the application.
// Fires ThemeChanged when the theme changes.
// MainWindow subscribes and sets RequestedThemeVariant programmatically.
//

using System;
using Avalonia.Styling;
using SunFlower.Client.Model;
using TextMateSharp.Grammars;

namespace SunFlower.Client.Service;

public class ThemeService
{
    private ThemeVariant _currentVariant = ThemeVariant.Default;
    private ThemeName _currentEditorVariant = ThemeName.Abbys;
    /// <summary>
    /// Fires when theme changes.
    /// </summary>
    public event Action<ThemeVariant>? ThemeChanged;
    /// <summary>
    /// Reacts when editor scheme changes
    /// </summary>
    public event Action<ThemeName>? EditorChanged;
    /// <summary>
    /// Current theme variant.
    /// </summary>
    public ThemeVariant CurrentVariant => _currentVariant;
    /// <summary>
    /// Current editor theme
    /// </summary>
    public ThemeName TextEditorVariant => _currentEditorVariant;

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

    public void SetEditor(ThemeName theme)
    {
        if (_currentEditorVariant == theme)
            return;

        _currentEditorVariant = theme;
        EditorChanged?.Invoke(theme);
    }
}