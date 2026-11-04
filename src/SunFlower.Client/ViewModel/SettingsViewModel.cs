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
        _installedFonts = FontManager
            .Current
            .SystemFonts
            .ToArray();
    }

    public string? HexFontFamily
    {
        get => _settings.HexControl?.Family;
        set
        {
            if (_settings.HexControl is null)
                _settings.HexControl = new Font(value ?? "Consolas", _settings.HexControl?.Size ?? 16);
            else
                _settings.HexControl.Family = value;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? HexFontSizeText
    {
        get => _settings.HexControl?.Size.ToString(CultureInfo.InvariantCulture) ?? 
               string.Empty;
        set
        {
            if (double.TryParse(value, out var size) && size > 0)
            {
                if (_settings.HexControl is null)
                    _settings.HexControl = new Font("Consolas", size);
                else
                    _settings.HexControl.Size = size;
            }

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? TextFontFamily
    {
        get => _settings.TextControl?.Family;
        set
        {
            if (_settings.TextControl is null)
                _settings.TextControl = new Font(value ?? "Droid Sans Mono", _settings.TextControl?.Size ?? 13);
            else
                _settings.TextControl.Family = value;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? TextFontSizeText
    {
        get => _settings.TextControl?.Size.ToString(CultureInfo.InvariantCulture) ?? 
               string.Empty;
        set
        {
            if (double.TryParse(value, out var size) && size > 0)
            {
                if (_settings.TextControl is null)
                    _settings.TextControl = new Font("Droid Sans Mono", size);
                else
                    _settings.TextControl.Size = size;
            }

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public string? HexBytesPerLineText
    {
        get => _settings.HexBytesPerLine?.ToString() ?? string.Empty;
        set
        {
            var text = value?.Trim();
            if (string.IsNullOrEmpty(text))
                _settings.HexBytesPerLine = null; // dynamic / automatic
            else if (int.TryParse(text, out var n) && n > 0)
                _settings.HexBytesPerLine = n;

            OnPropertyChanged();
            _settingsService.NotifySettingsChanged();
        }
    }

    public bool HexShowOffsetColumn
    {
        get => _settings.HexShowOffsetColumn;
        set { _settings.HexShowOffsetColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowHexColumn
    {
        get => _settings.HexShowHexColumn;
        set { _settings.HexShowHexColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowBinaryColumn
    {
        get => _settings.HexShowBinaryColumn;
        set { _settings.HexShowBinaryColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
    }

    public bool HexShowAsciiColumn
    {
        get => _settings.HexShowAsciiColumn;
        set { _settings.HexShowAsciiColumn = value; OnPropertyChanged(); _settingsService.NotifySettingsChanged(); }
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