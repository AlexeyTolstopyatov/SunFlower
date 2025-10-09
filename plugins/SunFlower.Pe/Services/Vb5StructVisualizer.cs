using System.Data;
using System.Text;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Headers;
using SunFlower.Abstractions;

namespace SunFlower.Pe.Services;

public class Vb5StructVisualizer(Vb5Header @struct) : AbstractStructVisualizer<Vb5Header>(@struct)
{
    private readonly string _heading = "### Visual Basic 5.0/6.0 Runtime section";
    public override DataTable ToDataTable()
    {
        var vb = new DataTable
        {
            Columns = { FlowerReport.ForColumnWith("Field", "?"), 
                FlowerReport.ForColumnWith("Data", "?") }
        };
        vb.Rows.Add(nameof(Vb5Header.VbMagic), FlowerReport.SafeString(new string(_struct.VbMagic)));
        vb.Rows.Add(nameof(Vb5Header.RuntimeBuild), "0x" + _struct.RuntimeBuild.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.LanguageDll), FlowerReport.SafeString(new string(_struct.LanguageDll)));
        vb.Rows.Add(nameof(Vb5Header.SecondLanguageDll),
            FlowerReport.SafeString(new string(_struct.SecondLanguageDll)));
        vb.Rows.Add(nameof(Vb5Header.RuntimeRevision), "0x" + _struct.RuntimeRevision.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.LanguageId), "0x" + _struct.LanguageId.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.SubMainAddress), "0x" + _struct.SubMainAddress.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectDataPointer), "0x" + _struct.ProjectDataPointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ControlsFlagLow), "0x" + _struct.ControlsFlagLow.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ControlsFlagHigh), "0x" + _struct.ControlsFlagHigh.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThreadFlags), "0x" + _struct.ThreadFlags.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThreadCount), "0x" + _struct.ThreadCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.FormCtlsCount), "0x" + _struct.FormCtlsCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ExternalCtlsCount), "0x" + _struct.ExternalCtlsCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ThunkCount), "0x" + _struct.ThunkCount.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.GuiTablePointer), "0x" + _struct.GuiTablePointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ExternalTablePointer), "0x" + _struct.ExternalTablePointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ComRegisterDataPointer), "0x" + _struct.ComRegisterDataPointer.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectDescriptionOffset), "0x" + _struct.ProjectDescriptionOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectExeNameOffset), "0x" + _struct.ProjectExeNameOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectHelpOffset), "0x" + _struct.ProjectHelpOffset.ToString("X"));
        vb.Rows.Add(nameof(Vb5Header.ProjectNameOffset), "0x" + _struct.ProjectNameOffset.ToString("X"));

        return vb;
    }

    public override Region ToRegion()
    {
        var content = new StringBuilder();

        content.AppendLine("If you see this section - this is already PE32 linked binary with embedded Microsoft Visual Basic runtime.");
        content.AppendLine("This structure is a part of VB 5.0 or a VB 6.0 runtime. It depends on target DLL which `@100` requires to correct run.");
        content.AppendLine($" - VBVM ver details. `__.__.{_struct.RuntimeBuild}.{_struct.RuntimeRevision}`");
        content.AppendLine($" - VBVM DLL: {FlowerReport.SafeString(new string(_struct.LanguageDll))}");
        
        return new Region(_heading, content.ToString(), ToDataTable());
    }
}