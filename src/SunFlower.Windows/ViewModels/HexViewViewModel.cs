using System.IO;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class HexViewViewModel : NotifyPropertyChanged
{
    public HexViewViewModel() : this("") {}
    public FileReader Reader { get; private set; }
    private readonly string _filePath;
    
    public HexViewViewModel(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return; // <-- ignore ViewModel requirements for 1st time
       
        _filePath = filePath;
        Reader = new(); // |<-- lifetime of stream must longer than UI constructor
        Reader.InitializeStream(filePath);
        
    }
}
