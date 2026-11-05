// CoffeeLake (C) 2026-*
// 
// The TextDocumentViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.ViewModel.Dock;

public partial class TextDocumentViewModel : FileDocument
{
    [ObservableProperty] private TextDocument? _textDocument;

    [ObservableProperty] private FontFamily? _textFontFamily;

    [ObservableProperty] private double _textFontSize;

    [ObservableProperty] private bool _textShowLineNumbers;
}