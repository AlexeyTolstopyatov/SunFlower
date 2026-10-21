// CoffeeLake (C) 2026-*
//
// DockingWorkspaceView - VS Code style docking workspace UserControl.
// Uses Dock.Avalonia DockControl to render the layout defined by
// DockingWorkspaceViewModel. Can be embedded in MainWindow via ViewLocator
// or opened in a separate WorkspaceWindow.
//

using Avalonia.Controls;

namespace SunFlower.Client.View;

public partial class DockingWorkspaceView : UserControl
{
    public DockingWorkspaceView()
    {
        InitializeComponent();
    }
}