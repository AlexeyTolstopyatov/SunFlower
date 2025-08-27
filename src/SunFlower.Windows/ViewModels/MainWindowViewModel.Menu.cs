using System.Data;
using System.Drawing;
using System.Windows.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using SunFlower.Readers;
using SunFlower.Services;
using SunFlower.Windows.Attributes;

namespace SunFlower.Windows.ViewModels;

/// <summary>
/// Part of <see cref="MainWindowViewModel"/> for Main Menu command bindings
/// </summary>
public partial class MainWindowViewModel
{
    public ICommand GetRecentFileCommand
    {
        get => _getRecentFileCommand;
        set => SetField(ref _getRecentFileCommand, value);
    }
    public ICommand GetFileCommand
    {
        get => _getFileCommand;
        set => SetField(ref _getFileCommand, value);
    }

    public ICommand GetProcessCommand
    {
        get => _getProcessCommand;
        set => SetField(ref _getProcessCommand, value);
    }

    private ICommand _getRegistryFileCommand;

    public ICommand GetRegistryFileCommand
    {
        get => _getRegistryFileCommand;
        set => SetField(ref _getRegistryFileCommand, value);
    }

    private ICommand _getFileCommand;
    private ICommand _getRecentFileCommand;
    private ICommand _getProcessCommand;
    private ICommand _getNotImplementedGrowlCommand;
    private ICommand _getMachineWordsCommand;
    private ICommand _clearRecentFilesCommand;
    private ICommand _clearRecentFileCommand;
    public ICommand ClearRecentFileCommand
    {
        get => _clearRecentFileCommand;
        set => SetField(ref _clearRecentFileCommand, value);
    }

    public ICommand GetDeleteRecentFilesCommandCommand
    {
        get => _clearRecentFilesCommand;
        set => SetField(ref _clearRecentFilesCommand, value);
    }

    public ICommand GetMachineWordsCommand
    {
        get => _getMachineWordsCommand;
        set => SetField(ref _getMachineWordsCommand, value);
    }

    public ICommand GetNotImplementedGrowlCommand
    {
        get => _getNotImplementedGrowlCommand;
        set => SetField(ref _getNotImplementedGrowlCommand, value);
    }

    #region Menu Callbacks
    /// <summary>
    /// Starts PropertiesWindow for recent file
    /// </summary>
    /// <param name="selectedRowView"></param>
    private void GetRecentFile(object selectedRowView)
    {
        try
        {
            var unboxed = (DataRowView)selectedRowView;

            FileName = unboxed.Row["Name"].ToString() ?? "<unknown>";
            FilePath = unboxed.Row["Path"].ToString() ?? string.Empty;
            TypeString = unboxed.Row["Type"].ToString() ?? string.Empty;
            Signature = unboxed["Sign"].ToString() ?? string.Empty;
            Size = unboxed.Row["Size"].ToString() ?? string.Empty;

            if (FilePath == string.Empty)
                return; // terminate "Call Editor"

            var inst = FlowerSeedManager.CreateInstance();
            Seeds = inst
                .LoadAllFlowerSeeds()
                .UpdateAllInvokedFlowerSeeds(FilePath)
                //.UnloadUnusedSeeds()
                .Seeds;
            
            WriteTracing(ref inst);
        }
        catch (Exception e)
        {
            Growl.ErrorGlobal(e.Message);
            return;
        }

        _windowManager.Show(this, new PropertiesWindow(), title: FilePath);
    }
    /// <summary>
    /// Calls <see cref="OpenFileDialog"/> instance and,
    /// Starts common reader (remembers general characteristics)
    /// and saves it to <c>recent.json</c>
    /// </summary>
    private void GetFile(object unused)
    {
        OpenFileDialog dialog = new()
        {
            Title = "Catch image by filename",
            Filter = "All files (*.*)|*.*",
            Multiselect = false
        };

        dialog.ShowDialog();

        if (dialog.FileName == string.Empty)
            return;

        // collect data-structure
        var result = FlowerBinarySeeker.Get(dialog.FileName);
        FileName = result.Name;
        FilePath = result.Path;
        TypeString = result.Type;
        Signature = result.Sign;
        Size = result.Size + "K"; // JS fell off
        
        _registryManager
            .SetFileName("recent")
          //.Create()
            .Create(result);
          //.OpenInWindowsNotepad();
        
        RecentTable = LoadRecentTableOnStartup(); // bad idea.
        
        // Extensions recall
        var inst = FlowerSeedManager.CreateInstance(); 
            
        Seeds = inst.LoadAllFlowerSeeds()
            .UpdateAllInvokedFlowerSeeds(dialog.FileName)
            .Seeds;
        
        WriteTracing(ref inst);
        
        // Call plugins Window/Main Workspace
        _windowManager.Show(this, new PropertiesWindow(), title: FilePath);
    }

    private void WriteTracing(ref FlowerSeedManager manager)
    {
        Tell("=== Kernel tracing ===");
        foreach (var message in manager.Messages)
        {
            Tell(message);
        }
        
        // information about external Exceptions
        Tell("=== Disabled plugins tracing ===");
        foreach (var plugin in Seeds.Where(plugin => !plugin.Status.IsEnabled))
        {
            Tell(plugin.Status.LastError is null ? $"?[{plugin.Seed}] has no result." : $"![{plugin.Seed}] " + plugin.Status.LastError.Message);
        }

    }
    /// <summary>
    /// Experimental feature (try to catch process by ID/Name)
    /// Requires Administrator permissions
    /// </summary>
    private void GetWin32Process(object unused)
    {
        Growl.WarningGlobal("Not implemented yet!");
    }
    /// <summary>
    /// Shows notification "Not implemented yet" at Desktop
    /// </summary>
    /// <param name="unused"></param>
    private void GetNotImplementedGrowl(object unused)
    {
        Growl.InfoGlobal("Not implemented yet");
    }

    /// <summary>
    /// Clear all recent files JSON list
    /// </summary>
    private void ClearRecentFiles()
    {
        RecentTable.Clear();
        
        _registryManager
            .SetFileName("recent")
            .Create();
    }
    /// <summary>
    /// Deletes current /selected in <see cref="RecentTable"/> row from JSON list
    /// and <see cref="RecentTable"/>
    /// </summary>
    /// <param name="file"></param>
    private void ClearRecentFile(object file)
    {
        try
        {
            // typeof(file) == DataRowView
            if (file is not DataRowView view) 
                return;
            
            view.Row.Delete();
            _registryManager
                .SetFileName("recent")
                .Delete(view.Row);
        }
        catch (Exception e)
        {
            Growl.WarningGlobal(e.Message);
        }
    }
    /// <summary>
    /// Opens file /in Windows Notepad/ by selected item CommandParameter
    /// </summary>
    /// <param name="name"></param>
    [Forgotten]
    private void OpenRegFileByName(object name)
    {
        try
        {
            _registryManager
                .SetFileName(name.ToString()!)
                .OpenInWindowsNotepad();
        }
        catch (Exception e)
        {
            Growl.WarningGlobal($"{e.Message}");
        }
    }
    #endregion
}