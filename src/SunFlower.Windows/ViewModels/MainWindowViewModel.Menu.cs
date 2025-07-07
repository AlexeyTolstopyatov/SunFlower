using System.IO;
using System.Windows.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SunFlower.Abstractions;
using SunFlower.Models;
using SunFlower.Services;

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

    private ICommand _getFileCommand;
    private ICommand _getRecentFileCommand;
    private ICommand _getProcessCommand;
    private ICommand _getNotImplementedGrowlCommand;
    private ICommand _getMachineWordsCommand;

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
    /// <param name="index"></param>
    private void GetRecentFile(object index)
    {
        MessageBox.Info(index.ToString(), "Selected item");
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

        ImageReaderResult result = ImageReader.GetImageResults(dialog.FileName);
        FileName = result.Name;
        FilePath = result.Path;
        Signature = result.Signature;
        Cpu = result.CpuArchitecture;
        
        // Write to recent table
        JArray resultList;
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "recent.json";
        
        if (File.Exists(filePath))
        {
            string recentJson = File.ReadAllText(filePath);
            resultList = JArray.Parse(recentJson);
            
            JObject openedFileObj = JObject.FromObject(result);
            resultList.Add(openedFileObj);
        
            File.WriteAllText(filePath, resultList.ToString(Formatting.Indented));
        }
        else
        {
            File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "recent.json");
            
            JObject openedFileObj = JObject.FromObject(result);
            resultList = [openedFileObj];

            File.WriteAllText(filePath, resultList.ToString(Formatting.Indented));
        }
        RecentTable = LoadRecentTableOnStartup(); // bad idea.
        
        // Extensions recall
        Seeds = FlowerSeedManager
            .CreateInstance() 
          //.LoadAllFlowerSeeds()
            .UpdateAllInvokedFlowerSeeds(dialog.FileName)
            .Seeds;
        
        // information about external Exceptions
        foreach (IFlowerSeed plugin in Seeds.Where(plugin => !plugin.Status.IsEnabled))
        {
            Tell(plugin.Status.LastError!.ToString());
        }
        
        // Call plugins Window/Main Workspace
        new PropertiesWindow().Show();
    }
    /// <summary>
    /// Experimental feature (try to catch process by ID/Name)
    /// Requires Administrator permissions
    /// </summary>
    private void GetWin32Process(object unused)
    {
        Growl.WarningGlobal("Administrator permissions required. Not implemented yet!");
    }
    /// <summary>
    /// Shows notification "Not implemented yet" at Desktop
    /// </summary>
    /// <param name="unused"></param>
    private void GetNotImplementedGrowl(object unused)
    {
        Growl.InfoGlobal("Not implemented yet");
    }
    #endregion
}