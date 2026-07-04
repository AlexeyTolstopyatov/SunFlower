// CoffeeLake (C) 2026-*
//
// WorkspaceView is the main workspace area as UserControl.
// Can be embedded in MainWindow via ViewLocator,
// or opened in a separate WorkspaceWindow.
//
using Avalonia.Controls;

namespace SunFlower.Client.View;

public partial class WorkspaceView : UserControl
{
    public WorkspaceView()
    {
        InitializeComponent();
    }
}