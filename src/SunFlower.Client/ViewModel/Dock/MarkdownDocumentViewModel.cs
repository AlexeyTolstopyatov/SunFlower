// CoffeeLake (C) 2026-*
// 
// The MarkdownDocumentViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.ViewModel.Dock;

public partial class MarkdownDocumentViewModel : FileDocument
{
    [ObservableProperty]
    private string? _content;
}