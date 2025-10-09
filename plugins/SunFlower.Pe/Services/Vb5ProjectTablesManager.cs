using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class Vb5ProjectTablesManager : DirectoryManager
{
    public string ProjectName { get; init; }
    public string ProjectExeName { get; init; }
    public string ProjectDescription { get; init; }
    public VbComRegistration Registration { get; init; }
    public VbComRegistrationInfo RegistrationInfo { get; init; }
    public Vb5ProjectInfo ProjectInfo { get; init; }

    public Vb5ProjectTablesManager(
        string path, 
        long vbnewOffset, 
        Vb5Header header, 
        FileSectionsInfo sectionsInfo) : base(sectionsInfo)
    {
        
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Strings'ch follows by the header are zero-terminated (or C-strings)
        
        var projDescriptionOffset = vbnewOffset + header.ProjectDescriptionOffset;
        var projNameOffset = vbnewOffset + header.ProjectNameOffset;
        var projExeNameOffset = vbnewOffset + header.ProjectExeNameOffset;

        
        ProjectName = FromCString(in reader, projNameOffset);
        ProjectExeName = FromCString(in reader, projExeNameOffset);
        ProjectDescription = FromCString(in reader, projDescriptionOffset);

        // external table
        var lpReg = header.ComRegisterDataPointer;
        stream.Position = lpReg;

        Registration = Fill<VbComRegistration>(reader);
        stream.Position += Registration.RegInfoOffset;
        
        RegistrationInfo = Fill<VbComRegistrationInfo>(reader);

        stream.Position = header.ProjectDataPointer;
        ProjectInfo = Fill<Vb5ProjectInfo>(reader);
        
    }

    private string FromCString(in BinaryReader reader, long offset)
    {
        reader.BaseStream.Position = offset;
        var b = reader.ReadChar();
        var stringBytes = new List<char>();
        while (b != '\0')
        {
            stringBytes.Add(b);
            b = reader.ReadChar();
        }

        return new string(stringBytes.ToArray());
    }
}