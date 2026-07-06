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
using SunFlower.Client.Services;
using System.Drawing.Text;
using System.Linq;
using Avalonia.Media;

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
    
    [RelayCommand]
    private void ChangeTheme(object? index)
    {
        if (index is not int)
            return;
        
        _themeService.SetTheme((Theme)index);
    }
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveAsync();
    }
}