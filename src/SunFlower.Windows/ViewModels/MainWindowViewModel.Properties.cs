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

    private string _fileName;
    private string _filePath;
    private string _signature;
    private string _cpu;
    
    private void CallEditor()
    {
        
    }
    
}