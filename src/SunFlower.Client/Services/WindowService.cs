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

namespace SunFlower.Client.Services;

public class WindowService
{
    /// <summary>
    /// Open the WorkspaceView in a separate window.
    /// </summary>
    public void OpenWorkspaceWindow(WorkspaceViewModel vm)
    {
        
        var window = new WorkspaceWindow
        {
            DataContext = vm
        };
        vm.ThisWindow = window;
        window.Show();
    }

    /// <summary>
    /// Close all workspace windows.
    /// </summary>
    public void CloseAllWorkspaceWindows()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var windows = desktop.Windows;
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i] is WorkspaceWindow)
                {
                    windows[i].Close();
                }
            }
        }
    }
}