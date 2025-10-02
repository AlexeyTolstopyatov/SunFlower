using System.Configuration;
using System.Text;
using System.Windows.Input;
using Microsoft.VisualBasic;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.Raw;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json.Converters;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class ConverterWindowViewModel : NotifyPropertyChanged
{
    public ConverterWindowViewModel()
    {
        _updateCommand = new ActionCommand(Update);
        _deleteSpacingCommand = new ActionCommand(DeleteSpacing);
        _reverseStringCommand = new ActionCommand(ReverseString);
    }
    
    private string _bytes = string.Empty;
    private string _target = string.Empty;
    private int _index = 0;
    private ICommand _updateCommand;
    private ICommand _deleteSpacingCommand;
    private ActionCommand _reverseStringCommand;
    public int Index
    {
        get => _index;
        set => SetField(ref _index, value);
    }
    public string Target
    {
        get => _target;
        set
        {
            Update();
            SetField(ref _target, value);
        }
    }

    public ActionCommand ReverseStringCommand
    {
        get => _reverseStringCommand;
        set => SetField(ref _reverseStringCommand, value);
    }
    public ICommand DeleteSpacingCommand
    {
        get => _deleteSpacingCommand;
        set => SetField(ref _deleteSpacingCommand, value);
    }
    public ICommand UpdateCommand
    {
        get => _updateCommand;
        set => SetField(ref _updateCommand, value);
    }
    public string Bytes
    {
        get => _bytes;
        set => SetField(ref _bytes, value);
    }
    

    private void Update()
    {
        Bytes = _index switch
        {
            0 => BitConverter.ToString(Encoding.ASCII.GetBytes(Target)),
            1 => BitConverter.ToString(Encoding.Unicode.GetBytes(Target)),
            2 => ToCorString(Target),
            _ => string.Empty
        };
    }

    private void ReverseString(object fieldIndex)
    {
        // if result -> reverse bytes. 55-53 -> 53-55
        if (fieldIndex.ToString() == "result")
            Bytes = Strings.StrReverse(Bytes);
        else
            Target = Strings.StrReverse(Target);
    }
    private void DeleteSpacing()
    {
        Bytes = new string(Bytes.Where(x => x != '-').Select(x => x).ToArray());
    }
    private static string ToCorString(string ascii)
    {
        if (ascii.Length % 2 != 0)
            return "";
        
        string a;
        try
        {
            a = string.Join("", Enumerable
                .Range(0, ascii.Length / 2)
                .Select(s => ascii.Substring(s * 2, 2))
                .Select(b => (char)Convert.ToByte(b, 0x10)));
        }
        catch (Exception e)
        {
            a = e.Message;
        }
        
        return a;
    }
}