using System.IO;

namespace SunFlower.Windows.Services;

public class FileReader
{
    public FileStream Stream { get; private set; }

    public BinaryReader Reader { get; private set; }
    public void InitializeStream(string path)
    {
        try
        {
            Stream = new(path, FileMode.Open, FileAccess.Read);
            Reader = new(Stream);
        }
        catch
        {
            // ignore
        }
    }
}