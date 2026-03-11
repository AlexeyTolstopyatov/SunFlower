using System.Data;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class DataGridWindowViewModel : NotifyPropertyChanged
{
    public DataGridWindowViewModel()
    {
        _table = ReadTable();
    }

    private DataTable _table;

    public DataTable Table
    {
        get => _table;
        set => SetField(ref _table, value);
    }

    private DataTable ReadTable()
    {
        DataTable t = new();

        RegistryManager
            .CreateInstance()
            .Of("sizes")
            .Fill(ref t);

        return t;
    }
}