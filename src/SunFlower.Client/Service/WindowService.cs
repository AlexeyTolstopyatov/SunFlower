//
// CoffeeLake (C) 2026-*
//
// WindowService opens new windows from ViewModel without View reference.
// ViewModel calls OpenWorkspaceWindow(), WindowService creates the Window.
//

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SunFlower.Client.View;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.Service;

public class WindowService
{
    /// <summary>
    ///     Open the DockWorkspaceView in a separate window.
    /// </summary>
    public void OpenWorkspaceWindow(DockWorkspaceViewModel vm)
    {
        var window = new WorkspaceWindow
        {
            DataContext = vm
        };

        // Pass both Window and DialogService references to the ViewModel
        vm.ThisWindow = window;
        vm.SetDialogService(window.DialogService);
        window.Show();
    }

    /// <summary>
    ///     Close all workspace windows.
    /// </summary>
    public void CloseAllWorkspaceWindows()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var windows = desktop.Windows;
        for (var i = windows.Count - 1; i >= 0; i--)
            if (windows[i] is WorkspaceWindow)
                windows[i].Close();
    }
}