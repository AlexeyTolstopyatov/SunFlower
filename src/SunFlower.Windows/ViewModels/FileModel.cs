namespace SunFlower.Windows.ViewModels;

public class FileModel : NotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _fullName = string.Empty;
    private string _size = string.Empty;
    private string _signature = string.Empty;
    private string _typeString = string.Empty;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetField(ref _fullName, value);
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

    public string TypeString
    {
        get => _typeString;
        set => SetField(ref _typeString, value);
    }
}