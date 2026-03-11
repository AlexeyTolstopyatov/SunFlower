using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class HexViewModel : NotifyPropertyChanged
{
    public HexViewModel() : this(string.Empty) { }
    public FileReader? Reader { get; }

    public HexViewModel(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return; // <-- ignore ViewModel requirements for 1st time

        Reader = new(); // |<-- lifetime of stream must longer than UI constructor
        Reader.InitializeStream(filePath);
    }
}
