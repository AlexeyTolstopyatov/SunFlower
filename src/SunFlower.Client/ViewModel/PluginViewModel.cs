// CoffeeLake (C) 2026-*
// 
// The PluginViewModel.cs represents view model for the PluginView control
// PluginView shows loaded plugins at the application startup
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Client.Services;
using SunFlower.Kernel.Readers;
using SunFlower.Kernel.Services;

namespace SunFlower.Client.ViewModel;

public partial class PluginViewModel : ObservableObject
{
    public PluginViewModel(PluginService? pluginService)
    {
        Assemblies = pluginService?.GetVersionInfo() ?? throw new NullReferenceException("Plugins service can't be not null!");
    }

    [ObservableProperty]
    private IReadOnlyList<FlowerVersionInfo> _assemblies;
}