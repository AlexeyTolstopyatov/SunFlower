// CoffeeLake (C) 2026-*
//
// WorkspaceWindow container for the workspace view.
// ViewModel opens it via a service, not by creating a Window directly.
//

using System;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using SunFlower.Client.Service;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.View;

public partial class WorkspaceWindow : Window
{
    private readonly Animation _dialogCloseAnimation;
    private readonly Animation _dialogOpenAnimation;
    private readonly SettingsService _settingsService;

    public WorkspaceWindow(SettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        DialogService = new DialogService(this);
        _dialogOpenAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.1),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.0),
                        new Setter(ScaleTransform.ScaleYProperty, 0.0)
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    },
                    KeyTime = TimeSpan.FromSeconds(0.1)
                }
            }
        };
        _dialogCloseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                        new Setter(ScaleTransform.ScaleXProperty, 1.0)
                    }
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, 0.0),
                        new Setter(ScaleTransform.ScaleXProperty, 0.0)
                    }
                }
            }
        };

        // Apply the command palette hotkey from settings (default Ctrl+Shift+P).
        UpdateCommandPaletteHotkey();

        // Re-apply when the user changes the hotkey in settings.
        _settingsService.SettingsChanged += UpdateCommandPaletteHotkey;
    }

    // Parameterless ctor so the XAML loader can instantiate the class via x:Class.
    public WorkspaceWindow() : this(new SettingsService())
    {
    }

    public DialogService DialogService { get; }

    private bool IsDialogAnimationEnabled => _settingsService.Current.DialogAnimationEnabled;

    /// <summary>
    ///     Parses the command palette hotkey from settings and applies it as a KeyBinding.
    /// </summary>
    private void UpdateCommandPaletteHotkey()
    {
        var hotkey = _settingsService.Current.CommandPaletteHotkey;

        // Remove any existing palette key bindings to avoid duplicates.
        var existing = RootGrid.KeyBindings
            .OfType<KeyBinding>()
            .Where(kb => kb.Command.ToString()?.Contains("BuildCommandPalette") == true)
            .ToList();
        
        foreach (var kb in existing)
            RootGrid.KeyBindings.Remove(kb);

        var gesture = ParseKeyGesture(hotkey);
        if (gesture is null)
            return;

        var vm = DataContext as DockWorkspaceViewModel;
        var command = vm?.BuildCommandPaletteCommand;
        if (command is null)
            return;

        RootGrid.KeyBindings.Add(new KeyBinding
        {
            Gesture = gesture,
            Command = command
        });
    }

    private static KeyGesture? ParseKeyGesture(string hotkey)
    {
        if (string.IsNullOrWhiteSpace(hotkey))
            return null;

        var parts = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return null;

        var modifiers = KeyModifiers.None;
        var key = Key.None;

        foreach (var part in parts)
        {
            var p = part.Trim();
            if (p.Equals("ctrl", StringComparison.OrdinalIgnoreCase))
                modifiers |= KeyModifiers.Control;
            else if (p.Equals("shift", StringComparison.OrdinalIgnoreCase))
                modifiers |= KeyModifiers.Shift;
            else if (p.Equals("alt", StringComparison.OrdinalIgnoreCase))
                modifiers |= KeyModifiers.Alt;
            else if (p.Equals("win", StringComparison.OrdinalIgnoreCase))
                modifiers |= KeyModifiers.Meta;
            else if (Enum.TryParse<Key>(p, true, out var parsedKey))
                key = parsedKey;
        }

        return key == Key.None 
            ? null 
            : new KeyGesture(key, modifiers);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Clean up project when window is closed
        if (DataContext is DockWorkspaceViewModel)
        {
            // The ViewModel shouldn't know about Window closing,
            // -> handle cleanup here via the service.
            // MainWindow handles the send dialog on app exit.
        }
    }

    private void DialogContainerClosed(object? sender, EventArgs e)
    {
        if (IsDialogAnimationEnabled)
            _dialogCloseAnimation.RunAsync(DialogPresenter);
        
        DialogLayer.IsVisible = false;
    }
    
    private void DialogContainerOpened(object? sender, EventArgs e)
    {
        DialogLayer.IsVisible = true;
        
        if (IsDialogAnimationEnabled)
            _dialogOpenAnimation.RunAsync(DialogPresenter);
    }

    private void DialogPresenterSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // _dialogOpenAnimation.RunAsync(DialogPresenter);
    }
}