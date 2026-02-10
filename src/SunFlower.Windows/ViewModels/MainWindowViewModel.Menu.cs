using System.Data;
using System.Windows.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using SunFlower.Readers;
using SunFlower.Services;
using SunFlower.Windows.Attributes;
using SunFlower.Windows.Views;

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

    private ICommand _getRegistryFileCommand;

    public ICommand GetConverterWindowCommand
    {
        get => _getConverterWindowCommand;
        set => SetField(ref _getConverterWindowCommand, value);
    }
    public ICommand GetRegistryFileCommand
    {
        get => _getRegistryFileCommand;
        set => SetField(ref _getRegistryFileCommand, value);
    }
    
    private ICommand _getConverterWindowCommand;
    private ICommand _getFileCommand;
    private ICommand _getRecentFileCommand;
    private ICommand _getNotImplementedGrowlCommand;
    private ICommand _getYourTableCommand;
    private ICommand _clearRecentFilesCommand;
    private ICommand _clearRecentFileCommand;
    private ICommand _getAboutCommand;

    public ICommand GetAboutCommand
    {
        get => _getAboutCommand;
        set => SetField(ref _getAboutCommand, value);
    }
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

    public ICommand GetYourTableCommand
    {
        get => _getYourTableCommand;
        set => SetField(ref _getYourTableCommand, value);
    }

    public ICommand GetNotImplementedGrowlCommand
    {
        get => _getNotImplementedGrowlCommand;
        set => SetField(ref _getNotImplementedGrowlCommand, value);
    }

    #region Menu Callbacks

    private void GetRecentFile(object selectedRowView)
    {
        try
        {
            var unboxed = (DataRowView)selectedRowView;

            Name = unboxed.Row["Name"].ToString() ?? "<unknown>";
            FullName = unboxed.Row["Path"].ToString() ?? string.Empty;
            TypeString = unboxed.Row["Type"].ToString() ?? string.Empty;
            Signature = unboxed["Sign"].ToString() ?? string.Empty;
            Size = unboxed.Row["Size"].ToString() ?? string.Empty;

            var model = new FileModel
            {
                Name = Name,
                FullName = FullName,
                Size = Size,
                Signature = Signature,
                TypeString = TypeString
            };
            
            if (FullName == string.Empty)
            {
                Growl.ErrorGlobal("Missing target path");
                return;
            }
            
            var workspaceVm = new WorkspaceViewModel(model);
            
            _windowManager.Show(
                workspaceVm,
                new WorkspaceWindow(),
                false,
                string.Empty
            );

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Growl.ErrorGlobal(e.Message);
        }
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
        Name = result.Name;
        FullName = result.Path;
        TypeString = result.Type;
        Signature = result.Sign;
        Size = $"{Math.Round(result.Size, 2)}K";
        
        _registryManager
            .Of("recent")
            .Create(result);
        
        RecentTable = LoadRecentTableOnStartup();
        
        // Extensions recall
        var inst = FlowerSeedManager.CreateInstance(); 
        var seeds = inst
            .LoadAllFlowerSeeds()
            .UpdateAllInvokedFlowerSeeds(dialog.FileName)
            .Seeds;
        
        foreach (var seed in seeds)
            Seeds.Add(seed);
        
        var model = new FileModel
        {
            Name = Name,
            FullName = FullName,
            Size = Size,
            Signature = Signature,
            TypeString = TypeString
        };

        // Call plugins Window/Main Workspace
        _windowManager.Show(
            new WorkspaceViewModel(model), 
            new WorkspaceWindow(),
            false,
            string.Empty
        );
    }
    
    /// <summary>
    /// Shows notification "Not implemented yet" at Desktop
    /// </summary>
    /// <param name="unused"></param>
    private void GetNotImplementedGrowl(object unused)
    {
        Growl.InfoGlobal("Not implemented yet");
    }

    private void GetAbout()
    {
        _windowManager.ShowUnmanaged(new AboutWindow(), false, "About");
    }
    /// <summary>
    /// Clear all recent files JSON list
    /// </summary>
    private void ClearRecentFiles()
    {
        RecentTable.Clear();
        
        _registryManager
            .Of("recent")
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
            if (file is not DataRowView view)
                return;
            
            _registryManager
                .Of("recent")
                .Delete(view.Row, out var isSuccess);
            
            view.Row.Delete();
            
            if (!isSuccess)
                throw new InvalidOperationException("Couldn't delete target from file");
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
                .Of(name.ToString()!)
                .OpenInWindowsNotepad();
        }
        catch (Exception e)
        {
            Growl.WarningGlobal($"{e.Message}");
        }
    }
    #endregion
}