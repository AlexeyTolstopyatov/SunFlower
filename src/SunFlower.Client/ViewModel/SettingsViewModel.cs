// CoffeeLake (C) 2026-*
// 
// The SettingsViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Model;
using System.Linq;
using Avalonia.Media;
using SunFlower.Client.Service;
using TextMateSharp.Grammars;

namespace SunFlower.Client.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ThemeService _themeService;
    
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
    
    [ObservableProperty]
    private SettingsModel _settings;
    
    [ObservableProperty]
    private FontFamily[] _installedFonts;
    
    [ObservableProperty]
    private Array _textEditorThemes = Enum.GetValues(typeof(ThemeName));

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
    }

    [RelayCommand]
    private async Task DeleteFirstCacheAsync()
    {
        await Console.Out.WriteLineAsync("First cache reacts");
        await _settingsService.DeleteFirstCacheAsync();
    }

    [RelayCommand]
    private async Task DeleteSecondCacheAsync()
    {
        await Console.Out.WriteLineAsync("Second cache reacts");
        await _settingsService.DeleteSecondCacheAsync();
    }
}