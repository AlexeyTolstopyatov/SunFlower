using System.Windows.Input;
using HandyControl.Controls;
using SunFlower.Windows.Views;

namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel
{
    public string FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public string FilePath
    {
        get => _filePath;
        set => SetField(ref _filePath, value);
    }

    public string Signature
    {
        get => _signature;
        set => SetField(ref _signature, value);
    }

    public string Cpu
    {
        get => _cpu;
        set => SetField(ref _cpu, value);
    }

    public string SignatureDWord
    {
        get => _signatureDWord;
        set => SetField(ref _signatureDWord, value);
    }
    
    private string _fileName;
    private string _filePath;
    private string _signature;
    private string _signatureDWord;
    private string _cpu;

    public ICommand CallEditorCommand
    {
        get => _callEditorCommand;
        set => SetField(ref _callEditorCommand, value);
    }

    private ICommand _callEditorCommand;
    /// <summary>
    /// Calls Monaco Editor window
    /// </summary>
    private void CallEditor()
    {
        _windowManager.ShowUnmanaged(new MonacoWindow(
            Seeds
            .Where(s => s.Status.IsEnabled)
            .ToList()), 
            title: FilePath, 
            isDialog: false);
    }
}