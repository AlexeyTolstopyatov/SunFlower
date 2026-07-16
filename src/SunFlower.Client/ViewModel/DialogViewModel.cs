// CoffeeLake (C) 2026-*
// 
// The DialogViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Client.Service;

namespace SunFlower.Client.ViewModel;

public class DialogViewModel : ObservableObject
{
    public DialogService? DialogService { get; set; }
}