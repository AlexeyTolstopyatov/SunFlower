// CoffeeLake (C) 2026-*
// 
// The DialogViewModel.cs represents embedded dialogs context class 
// Dialog container is a special Popup control which contains prepared Flyout 
// 
// Once thing what will be customized is a dialog. UserControl designed with XAML
// and bound dialog context class which must inherit this dialog context base.
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Client.Service;

namespace SunFlower.Client.ViewModel;

public class DialogViewModel : ObservableObject
{
    public DialogService? DialogService { get; set; }
}