using System.Data;
using System.Text;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class PeStructVisualizer(PeFileHeader s) : AbstractStructVisualizer<PeFileHeader>(s)
{
    public readonly string _content = "## Portable Executable Header";
    private readonly PeFileHeader _s = s;

    public override DataTable ToDataTable()
    {
        var coff = new DataTable
        {
            Columns = { "Field:?", "Data:?" }
        };

        coff.Rows.Add("Machine", "0x" + _s.Machine.ToString("X"));
        coff.Rows.Add("NumberOfSections", "0x" + _s.NumberOfSections.ToString("X"));
        coff.Rows.Add("TimeDataStamp", DateTime.FromBinary(_s.TimeDateStamp) + $"(0x{_s.TimeDateStamp:X})");
        coff.Rows.Add("PointerToSymbolTable", $"0x{_s.PointerToSymbolTable:X}");
        coff.Rows.Add("NumberOfSymbols", $"0x{_s.NumberOfSymbols:X}");
        coff.Rows.Add("SizeOfOptionalHeader", $"0x{_s.SizeOfOptionalHeader:X}");
        coff.Rows.Add("Characteristics", $"0x{_s.Characteristics:X}");
        
        return coff;
    }

    public override string ToString()
    {
        var content = new StringBuilder();
        content.AppendLine("The main header of every PE linked object. Magic signature `PE00` is constant.");
        content.AppendLine("This value independent on `BYTE` or a `WORD` ordering");
        content.AppendLine("Other names: `FILE HEADER`, `COFF HEADER` and others.");
        
        
        return content.ToString();
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}