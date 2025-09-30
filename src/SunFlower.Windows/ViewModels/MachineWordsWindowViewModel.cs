using System.Data;
using SunFlower.Windows.Services;

namespace SunFlower.Windows.ViewModels;

public class MachineWordsWindowViewModel : NotifyPropertyChanged
{
    public MachineWordsWindowViewModel()
    {
        _machineWordsTable = GetMachineWordsAndSizes();
    }

    private DataTable _machineWordsTable;
    
    public DataTable MachineWordsTable
    {
        get => _machineWordsTable;
        set => SetField(ref _machineWordsTable, value);
    }

    private DataTable GetMachineWordsAndSizes()
    {
        DataTable namesAndSizes = new();
        
        RegistryManager.CreateInstance()
            .Of("sizes")
            .Fill(ref namesAndSizes);

        return namesAndSizes;
    }
}