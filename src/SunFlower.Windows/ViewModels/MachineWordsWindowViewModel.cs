using System.Data;

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
        DataTable namesAndSizes = new("IA-32 Machine Words and Sizes")
        {
            Columns =
            {
                "CPU",
                "bOffset",
                "wOffset",
                "dwOffset",
                "qwOffset",
                "szOffset"
            }
        };
        namesAndSizes.Rows.Add("Intel i8080 (8-bit)", 8, 8, 0, 0, 8);
        namesAndSizes.Rows.Add("Intel i8086 (16-bit)", 8, 16, 0, 0, 16);
        namesAndSizes.Rows.Add("Intel x86", 8, 16, 32, 0, 32);
        namesAndSizes.Rows.Add("Intel x86-64", 8, 16, 32, 64, 64);

        return namesAndSizes;
    }
}