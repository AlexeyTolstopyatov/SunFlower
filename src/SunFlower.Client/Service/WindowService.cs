//
// CoffeeLake (C) 2026-*
//
// WindowService opens new windows from ViewModel without View reference.
// ViewModel calls OpenWorkspaceWindow(), WindowService creates the Window.
//

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaEdit.TextMate;
using SunFlower.Client.View;
using SunFlower.Client.ViewModel;
using TextMateSharp.Grammars;

namespace SunFlower.Client.Service;

public class WindowService
{
    /// <summary>
    /// Open the WorkspaceView in a separate window.
    /// </summary>
    public void OpenWorkspaceWindow(WorkspaceViewModel vm)
    {
        // Workspace control stores inside the window -> define the user control in it
        var window = new WorkspaceWindow
        {
            DataContext = vm
        };
        
        var inst = window.WorkspaceView.Editor.InstallTextMate(new RegistryOptions(App.ThemeService.TextEditorVariant));
        var grammar = Path.Combine(AppContext.BaseDirectory, "Grammar", "intel.json");
        inst.SetGrammarFile(grammar);
        
        App.ThemeService.EditorChanged += variant =>
        {
            window.WorkspaceView.Editor.InstallTextMate(new RegistryOptions(variant));
        };
        
        // Pass both Window and DialogService references to the ViewModel
        vm.ThisWindow = window;
        vm.SetDialogService(window.DialogService);
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
