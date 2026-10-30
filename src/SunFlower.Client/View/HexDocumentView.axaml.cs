using Avalonia.Controls;
using Avalonia.Interactivity;
using SunFlower.Client.ViewModel.Dock;

namespace SunFlower.Client.View;

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
            vm.Editor = Editor;
    }
}