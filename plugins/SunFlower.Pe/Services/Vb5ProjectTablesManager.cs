using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class Vb5ProjectTablesManager : UnsafeManager
{
    public string ProjectName { get; init; }
    public string ProjectExeName { get; init; }
    public string ProjectDescription { get; init; }

    public Vb5ProjectTablesManager(string path, long vbnewOffset, Vb5Header header)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Strings 'ch follows by the header are zero-terminated (or C-strings)
        
        var projDescriptionOffset = vbnewOffset + header.ProjectDescriptionOffset;
        var projNameOffset = vbnewOffset + header.ProjectNameOffset;
        var projExeNameOffset = vbnewOffset + header.ProjectExeNameOffset;

        
        ProjectName = FromCString(in reader, projNameOffset);
        ProjectExeName = FromCString(in reader, projExeNameOffset);
        ProjectDescription = FromCString(in reader, projDescriptionOffset);

        // external table
        
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