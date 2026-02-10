namespace SunFlower.Windows.ViewModels;

public partial class MainWindowViewModel
{
    private string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    private string FullName
    {
        get => _fullName;
        set => SetField(ref _fullName, value);
    }

    private string TypeString
    {
        get => _typeString;
        set => SetField(ref _typeString, value);
    }

    private string Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    private string Signature
    {
        get => _signature;
        set => SetField(ref _signature, value);
    }

    private string _name;
    private string _fullName;
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
}