using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Vb4StructVisualizer(Vb4Header @struct) : AbstractStructVisualizer<Vb4Header>(@struct)
{
    private readonly string _content = "### Visual Basic 4.0 Unofficial Section";
    private readonly Vb4Header _struct1 = @struct;

    public override DataTable ToDataTable()
    {
        var vb4 = new DataTable
        {
            Columns =
            {
                FlowerReport.ForColumnWith("Field", "?"),
                FlowerReport.ForColumnWith("Value", "?")
            }
        };

        vb4.Rows.Add(FlowerReport.ForColumn("Magic?", typeof(string)),
            FlowerReport.SafeString(new string(_struct1.Signature)));
        vb4.Rows.Add(FlowerReport.ForColumn("?_1", typeof(ushort)), "0x" + _struct1.Undefined1.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_2", typeof(ushort)), "0x" + _struct1.Undefined2.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_3", typeof(ushort)), "0x" + _struct1.Undefined3.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_4", typeof(ushort)), "0x" + _struct1.Undefined4.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_5", typeof(ushort)), "0x" + _struct1.Undefined5.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_6", typeof(ushort)), "0x" + _struct1.Undefined6.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_7", typeof(ushort)), "0x" + _struct1.Undefined7.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_8", typeof(ushort)), "0x" + _struct1.Undefined8.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_9", typeof(ushort)), "0x" + _struct1.Undefined9.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_10",typeof(ushort)), "0x" + _struct1.Undefined10.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_11",typeof(ushort)), "0x" + _struct1.Undefined11.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_12",typeof(ushort)), "0x" + _struct1.Undefined12.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_13",typeof(ushort)), "0x" + _struct1.Undefined13.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_14",typeof(ushort)), "0x" + _struct1.Undefined14.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_15",typeof(ushort)), "0x" + _struct1.Undefined15.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("LanguageDLLId", typeof(ushort)),
            "0x" + _struct1.LanguageDllId.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_16",typeof(ushort)), "0x" + _struct1.Undefined16.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_17",typeof(ushort)), "0x" + _struct1.Undefined17.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_18",typeof(ushort)), "0x" + _struct1.Undefined18.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("SubMainAddress", typeof(uint)),
            "0x" + _struct1.SubMainAddress.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("Address", typeof(uint)),
            "0x" + _struct1.Address.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_21",typeof(ushort)), "0x" + _struct1.Undefined21.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_22",typeof(ushort)), "0x" + _struct1.Undefined22.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_23",typeof(ushort)), "0x" + _struct1.Undefined23.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_24",typeof(ushort)), "0x" + _struct1.Undefined24.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_25",typeof(ushort)), "0x" + _struct1.Undefined25.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("?_26",typeof(ushort)), "0x" + _struct1.Undefined26.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExeNameLength", typeof(ushort)),
            "0x" + _struct1.ExeNameLength.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ProjNameLength", typeof(ushort)),
            "0x" + _struct1.ProjectNameLength.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("FormsCount", typeof(ushort)),
            "0x" + _struct1.FormsCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ModulesClassesCount", typeof(ushort)), "0x" + _struct1.ModulesClassesCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExternalControlsCount", typeof(ushort)), "0x" + _struct1.ExternComponentsCount.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("Foreach file equals 0x176d", typeof(ushort)),
            "0x" + _struct1.InEachFile176d.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("GuiTableOffset", typeof(uint)),
            "0x" + _struct1.GuiTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("???_TableOffset", typeof(uint)),
            "0x" + _struct1.UndefinedTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ExternalControlsTableOffset", typeof(uint)),
            "0x" + _struct1.ExternComponentTableOffset.ToString("X"));
        vb4.Rows.Add(FlowerReport.ForColumn("ProjInfoTableOffset", typeof(uint)),
            "0x" + _struct1.ProjectInfoTableOffset.ToString("X"));


        return vb4;
    }

    public override string ToString()
    {
        return "This is a section that bases on the legacy by `VBGamer 45` and `DoDi` placed here." + 
               "I'm trying to demangle and define other undocumented leaked structure fields. for PE32 linked programs" +
               "So, If you see this section - you must know this is a very rare artifact.";
    }

    public override Region ToRegion()
    {
        return new Region(_content, ToString(), ToDataTable());
    }
}