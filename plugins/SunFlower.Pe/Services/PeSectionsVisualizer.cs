using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class PeSectionsVisualizer(PeSection[] @struct) : AbstractStructVisualizer<PeSection[]>(@struct)
{
    private readonly string _content = "## Sections";
    public override DataTable ToDataTable()
    {
        DataTable sections = new()
        {
            TableName = "Sections Summary",
            Columns = 
            {
                "Name:s", 
                "VirtualAddress:4",
                "VirtualSize:4",
                "SizeOfRawData:4",
                "*RawData:4", // Pointer => *
                "*Relocs:4",
                "*Line#:4",
                "#Relocs:2",
                "#LineNumbers:2",
                "Characteristics:4" 
            }
        };
        
        foreach (var dump in _struct)
        {
            sections.Rows.Add(
                new String(dump.Name.Where(x => x != '\0').ToArray()),
                "0x" + dump.VirtualAddress.ToString("X"),
                "0x" + dump.VirtualSize.ToString("X"),
                "0x" + dump.SizeOfRawData.ToString("X"),
                "0x" + dump.PointerToRawData.ToString("X"),
                "0x" + dump.PointerToRelocations.ToString("X"),
                "0x" + dump.PointerToLinenumbers.ToString("X"),
                "0x" + dump.NumberOfRelocations.ToString("X"),
                "0x" + dump.NumberOfLinenumbers.ToString("X"),
                "0x" + dump.Characteristics.ToString("X")
            );
        }

        return sections;
    }

    public override string ToString()
    {
        return @"Each row of the section table is, in effect, a section header. 
This table immediately follows the optional header, if any.
This positioning is required because the file header does not contain a direct pointer to the section table.
Instead, the location of the section table is determined by calculating the location of the first byte after
the headers. Make sure to use the size of the optional header as specified in the file header.
The number of entries in the section table is given by the `NumberOfSections` field in the file header. 
Entries in the section table are numbered starting from one.
The code and data memory section entries are in the order chosen by the linker.";
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}