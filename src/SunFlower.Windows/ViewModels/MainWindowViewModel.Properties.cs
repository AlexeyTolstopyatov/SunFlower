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

    public string TypeString
    {
        get => _typeString;
        set => SetField(ref _typeString, value);
    }

    public string Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    public string Signature
    {
        get => _signature;
        set => SetField(ref _signature, value);
    }
    
    private string _fileName;
    private string _filePath;
    private string _typeString;
    private string _signature;
    private string _size;
    private bool _isReady;

    /// <summary>
    /// Binds to "CallMonaco" button.
    /// If enabled plugins = 0 -> holds "false"
    /// </summary>
    public bool IsReady
    {
        get => _isReady;
        set
        {
            _isReady = value;
            
            if (Seeds.All(s => s.Status.IsEnabled))
                SetField(ref _isReady, true);
            else 
                SetField(ref _isReady, false);
        }
    }

    public ICommand CallEditorCommand
    {
        get => _callEditorCommand;
        set => SetField(ref _callEditorCommand, value);
    }

    public ICommand CallHexViewerCommand
    {
        get => _callHexViewerCommand;
        set => SetField(ref _callHexViewerCommand, value);
    }
    private ICommand _callEditorCommand;
    private ICommand _callHexViewerCommand;
    
    /// <summary>
    /// Calls Monaco Editor window
    /// </summary>
    private void CallEditor()
    {
        try
        {
            _windowManager.ShowUnmanaged(new MonacoWindow(
                    Seeds
                        .Where(s => s.Status.IsEnabled)
                        .ToList()),
                title: FilePath,
                isDialog: false);
        }
        catch (Exception e)
        {
            Growl.ErrorGlobal(e.Message);
            Tell(e.ToString());
        }
    }

    private void CallViewer()
    {
        _windowManager.ShowUnmanaged(new HexViewerWindow()
        {
            DataContext = new HexViewViewModel(_filePath)
        }, false, _filePath);
    }
}