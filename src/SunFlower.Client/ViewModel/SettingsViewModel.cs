// CoffeeLake (C) 2026-*
// 
// The SettingsViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Model;
using SunFlower.Client.Service;
using TextMateSharp.Grammars;

namespace SunFlower.Client.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ThemeService _themeService;

    [ObservableProperty] private bool _firstCacheCleared;
    [ObservableProperty] private FontFamily[] _installedFonts;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private SettingsModel _settings;
    [ObservableProperty] private bool _settingsSaved;
    [ObservableProperty] private Array _textEditorThemes = Enum.GetValues(typeof(ThemeName));

    public SettingsViewModel(SettingsService settingsService, ThemeService themeService)
    {
        _settingsService = settingsService;
        _themeService = themeService;

        Settings = settingsService.Current;
        InstalledFonts = FontManager
            .Current
            .SystemFonts
            .ToArray();
    }

    public string? HexFontFamily
    {
        get => Settings.HexControl?.Family;
        set
        {
            if (Settings.HexControl is null)
                Settings.HexControl = new Font(value ?? "Consolas", Settings.HexControl?.Size ?? 16);
            else
                Settings.HexControl.Family = value;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? HexFontSizeText
    {
        get => Settings.HexControl?.Size.ToString(CultureInfo.InvariantCulture) ?? 
               string.Empty;
        set
        {
            if (double.TryParse(value, out var size) && size > 0)
            {
                if (Settings.HexControl is null)
                    Settings.HexControl = new Font("Consolas", size);
                else
                    Settings.HexControl.Size = size;
            }

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? TextFontFamily
    {
        get => Settings.TextControl?.Family;
        set
        {
            if (Settings.TextControl is null)
                Settings.TextControl = new Font(value ?? "Droid Sans Mono", Settings.TextControl?.Size ?? 13);
            else
                Settings.TextControl.Family = value;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? TextFontSizeText
    {
        get => Settings.TextControl?.Size.ToString(CultureInfo.InvariantCulture) ?? 
               string.Empty;
        set
        {
            if (double.TryParse(value, out var size) && size > 0)
            {
                if (Settings.TextControl is null)
                    Settings.TextControl = new Font("Droid Sans Mono", size);
                else
                    Settings.TextControl.Size = size;
            }

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? HexBytesPerLineText
    {
        get => Settings.HexBytesPerLine?.ToString() ?? string.Empty;
        set
        {
            var text = value?.Trim();
            if (string.IsNullOrEmpty(text))
                Settings.HexBytesPerLine = null; // dynamic / automatic
            else if (int.TryParse(text, out var n) && n > 0)
                Settings.HexBytesPerLine = n;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public bool HexShowOffsetColumn
    {
        get => Settings.HexShowOffsetColumn;
        set { Settings.HexShowOffsetColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowHexColumn
    {
        get => Settings.HexShowHexColumn;
        set { Settings.HexShowHexColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowBinaryColumn
    {
        get => Settings.HexShowBinaryColumn;
        set { Settings.HexShowBinaryColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowAsciiColumn
    {
        get => Settings.HexShowAsciiColumn;
        set { Settings.HexShowAsciiColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool DialogAnimationEnabled
    {
        get => Settings.DialogAnimationEnabled;
        set { Settings.DialogAnimationEnabled = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public string CommandPaletteHotkey
    {
        get => Settings.CommandPaletteHotkey;
        set { Settings.CommandPaletteHotkey = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool TextShowLineNumbers
    {
        get => Settings.ShowLineNumbers;
        set { Settings.ShowLineNumbers = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    [RelayCommand]
    private void ChangeTheme(object? index)
    {
        if (index is not int)
            return;

        _themeService.SetTheme((Theme)index);
    }

    [RelayCommand]
    private void ChangeEditorTheme(object? index)
    {
        if (index is null)
            return;

        _themeService.SetEditor((ThemeName)index);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveAsync();

        FirstCacheCleared = true;
        Message = "Settings saved!";

        await Task.Delay(200);

        Message = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteCacheAsync()
    {
        await _settingsService.DeleteCache();

        FirstCacheCleared = true;

        Message = "Cache is removed!";
        await Task.Delay(200);
        
        Message = string.Empty;
    }
}