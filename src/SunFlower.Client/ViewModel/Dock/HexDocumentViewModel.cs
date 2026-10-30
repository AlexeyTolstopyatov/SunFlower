// CoffeeLake (C) 2026-*
// 
// The HexDocumentViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using Avalonia.Media;
using AvaloniaHex;
using AvaloniaHex.Document;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunFlower.Client.ViewModel.Dock;


public partial class HexDocumentViewModel : FileDocument
{
    [ObservableProperty] 
    private IBinaryDocument? _binaryDocument;
    
    [ObservableProperty]
    private double _hexFontSize;
    
    [ObservableProperty]
    private FontFamily? _hexFontFamily;
    
    public HexEditor? Editor { get; set; }
}