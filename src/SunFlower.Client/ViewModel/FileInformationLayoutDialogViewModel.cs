// CoffeeLake (C) 2026-*
// 
// The FileInformationLayoutDialogViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using SunFlower.Kernel.Readers;

namespace SunFlower.Client.ViewModel;

public partial class FileInformationLayoutDialogViewModel(string path) : DialogViewModel
{
    // If "Undefined" -> mark as null
    [ObservableProperty]
    private FlowerFileInfo? _flowerFileInfo = FlowerBinarySeeker.Get(path);
}