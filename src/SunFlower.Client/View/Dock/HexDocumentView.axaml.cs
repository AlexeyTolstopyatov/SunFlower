using Avalonia.Controls;
using Avalonia.Interactivity;
using SunFlower.Client.ViewModel.Dock;

namespace SunFlower.Client.View.Dock;

public partial class HexDocumentView : UserControl
{
    public HexDocumentView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HexDocumentViewModel vm)
        {
            vm.Editor = Editor;

            // Apply pending bytes-per-line (fixed or dynamic) now that the editor exists.
            Editor.HexView.BytesPerLine = vm.BytesPerLine;
        }
    }
}