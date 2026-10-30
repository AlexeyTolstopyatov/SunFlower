// CoffeeLake (C) 2026-*
//
// DockWorkspaceView - docking workspace UserControl.
// Uses Dock.Avalonia DockControl to render the layout defined by
// DockWorkspaceViewModel. Can be embedded in MainWindow via ViewLocator
// or opened in a separate WorkspaceWindow.
//

using Avalonia.Controls;

namespace SunFlower.Client.View;

public partial class DockWorkspaceView : UserControl
{
    public DockWorkspaceView()
    {
        InitializeComponent();
    }
}