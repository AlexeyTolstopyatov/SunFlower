// CoffeeLake (C) 2026-*
//
// WorkspaceWindow container for the workspace view.
// ViewModel opens it via a service, not by creating a Window directly.
//
using Avalonia.Controls;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.View;

public partial class WorkspaceWindow : Window
{
    public WorkspaceWindow()
    {
        InitializeComponent();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Clean up project when window is closed
        if (DataContext is WorkspaceViewModel vm)
        {
            // The ViewModel shouldn't know about Window closing,
            // -> handle cleanup here via the service.
            // MainWindow handles the save dialog on app exit.
        }
    }
}