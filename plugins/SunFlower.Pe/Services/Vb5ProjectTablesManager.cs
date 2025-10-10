using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;

public class VbImageInfo(string path, long offset, Vb5Header vb5Header, FileSectionsInfo info)
{
    public FileSectionsInfo Info { get; init; } = info;
    public string Path { get; init; } = path;
    public Vb5Header Vb5Header { get; init; } = vb5Header;
    public long Vb5HeaderOffset { get; init; } = offset;
}

public class Vb5ProjectTablesManager : DirectoryManager
{
    public string ProjectName { get; }
    public string ProjectExeName { get; }
    public string ProjectDescription { get; }
    public VbComRegistration Registration { get; }
    public VbComRegistrationInfo RegistrationInfo { get; }
    public Vb5ProjectInfo ProjectInfo { get; }

    
    /// <summary>
    /// Avoid SectionNotFoundException and permanent retranslate
    /// the VA/RVA pointers to raw positions 
    /// </summary>
    /// <returns></returns>
    private long Shift(long offset)
    {
        // I've seen it in nightmare and this idea works for *LONG pointers...
        return ((offset - _imageBase) < 0) switch 
        {
            true => _imageBase - offset,
            false => offset - _imageBase 
        };
    }

    private readonly long _imageBase;
    public Vb5ProjectTablesManager(
        string path, 
        long vbnewOffset, 
        Vb5Header header, 
        FileSectionsInfo sectionsInfo) : base(sectionsInfo)
    {
        _imageBase = sectionsInfo.ImageBase;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Strings'ch follows by the header are zero-terminated (or C-strings)
        
        var projDescriptionOffset = vbnewOffset + header.ProjectDescriptionOffset;
        var projNameOffset = vbnewOffset + header.ProjectNameOffset;
        var projExeNameOffset = vbnewOffset + header.ProjectExeNameOffset;

        
        ProjectName = FromCString(in reader, projNameOffset);
        ProjectExeName = FromCString(in reader, projExeNameOffset);
        ProjectDescription = FromCString(in reader, projDescriptionOffset);

        // VA/RVA pointers needs to be checked and translated before usage
        
        var lpReg = Shift(header.ComRegisterDataPointer);
        stream.Position = lpReg;

        Registration = Fill<VbComRegistration>(reader);
        stream.Position = Shift(Registration.RegInfoOffset);
        
        RegistrationInfo = Fill<VbComRegistrationInfo>(reader);

        stream.Position = Shift(header.ProjectDataPointer);
        ProjectInfo = Fill<Vb5ProjectInfo>(reader);
        
        reader.Close();
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