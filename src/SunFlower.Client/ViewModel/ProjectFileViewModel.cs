// CoffeeLake (C) 2026-*
// 
// The ProjectFileViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Client.ViewModel.Dock;

namespace SunFlower.Client.ViewModel;

public partial class ProjectFileViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _fullPath;
}